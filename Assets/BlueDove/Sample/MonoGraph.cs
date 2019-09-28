using System;
using System.Collections.Generic;
using System.Linq;
using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using Microsoft.Collections.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace BlueDove.Sample
{
    public class MonoGraph : MonoBehaviour, IGraph<MonoNode, MonoEdge>, ICostFunc<MonoEdge>
    {
        private int idCounter;

        [FormerlySerializedAs("prefab")] [SerializeField]
        private MonoNode nodePrefab;

        [SerializeField] private MonoEdge edgePrefab;
        private DictionarySlim<MonoNode, List<MonoEdge>> _dictionary;
        [SerializeField] private bool _autoCreateEdges;
        [SerializeField] private float minDistSq = 20f;
        [SerializeField] private float minAngle = 30f;
        [SerializeField] private Color edgeSourceDefault;
        [SerializeField] private Color edgeTargetDefault;

        public MonoNode CreateNewNode(Vector3 pos)
        {
            var node = Instantiate(nodePrefab, pos, Quaternion.identity);
            node.SetID(++idCounter);
            _dictionary.GetOrAddValueRef(node) = new List<MonoEdge>();
            return node;
        }

        public MonoEdge CreateEdge(MonoNode source, MonoNode target)
        {
            var edge = Instantiate(edgePrefab);
            edge.Source = source;
            edge.Target = target;
            AddEdge(edge);
            return edge;
        }

        private void CreateEdges(float minDistSq, float minAngle, Func<MonoNode, MonoNode, MonoEdge> func)
        {
            var nodes = GetNodes().ToArray();
            var dict = new DictionarySlim<MonoNode, List<(MonoNode, Vector3)>>();
            var removeList = new List<int>();
            var remove2List = new List<(MonoNode, MonoNode)>();
            for (var i = 0; i < nodes.Length; i++)
            {
                var nodeA = nodes[i];
                ref var list = ref dict.GetOrAddValueRef(nodeA);
                if (list == null) list = new List<(MonoNode, Vector3)>();
                for (var j = i + 1; j < nodes.Length; j++)
                {
                    var nodeB = nodes[j];
                    var vec = nodeA.transform.position - nodeB.transform.position;
                    var distSq = Vector3.SqrMagnitude(vec);
                    if (distSq > minDistSq)
                        continue;
                    removeList.Clear();
                    for (var k = 0; k < list.Count; k++)
                    {
                        var (nodeC, vector3) = list[k];
                        var angle = Vector3.Angle(vec, vector3);
                        if (angle >= minAngle) continue;
                        if (angle > 180 - (minAngle))
                        {
                            //var sqN = Vector3.SqrMagnitude(nodeC.transform.position - nodeB.transform.position);
                            //if (sqN <= distSq)
                            {
                                remove2List.Add((nodeB, nodeC));
                            }
                        }
                        else
                        {
                            var distX = Vector3.SqrMagnitude(vector3);
                            if (distX > distSq)
                                removeList.Add(k);
                            else
                                goto NoAdd;
                        }
                    }

                    for (var k = 0; k < removeList.Count; k++)
                    {
                        if(removeList[k] < 0) continue;
                        list.RemoveAt(removeList[k]);
                    }

                    list.Add((nodeB, vec));

                    for (int k = 0; k < remove2List.Count; k++)
                    {
                        var (item1, item2) = remove2List[k];
                        if (!dict.TryGetValue(item1, out var listX)) continue;
                        var x = listX.FindIndex(y => y.Item1.Equals(item2));
                        if (x != 1)
                            listX.RemoveAt(x);
                        else
                        {
                            if (!dict.TryGetValue(item2, out var listY)) continue;
                            var p = listY.FindIndex(q => q.Item1.Equals(item1));
                            if (p != 1)
                            {
                                listY.RemoveAt(p);
                            }
                        }
                    }
                    
                    NoAdd: ;
                }
            }

            foreach (var pair in dict)
            {
                foreach (var tuple in pair.Value)
                {
                    AddEdge(func(pair.Key, tuple.Item1));
                }
            }
        }

        private void Awake()
        {
            edgeSourceDefault = new Color32(0x5E, 0x15, 0x15, byte.MaxValue);
            edgeTargetDefault = new Color32(0xD1, 0xD1, 0xD1, byte.MaxValue);
        }
        

        // Start is called before the first frame update
        void Start()
        {
            _dictionary = new DictionarySlim<MonoNode, List<MonoEdge>>();
            idCounter = 0;
            InitChildNodes();
            if (_autoCreateEdges)
            {
                CreateEdges(minDistSq, minAngle, (x, y) =>
                {
                    var e = Instantiate(edgePrefab, transform);
                    e.Source = x;
                    e.Target = y;
                    e.name = $"Edge [{e.Source.name}, {e.Target.name}]";
                    return e;
                });
            }
            else
            {
                InitChildEdges();
            }
        }
        

        void InitChildNodes()
        {
            var nodes = GetComponentsInChildren<MonoNode>();
            foreach (var node in nodes)
            {
                node.SetID(++idCounter);
                _dictionary.GetOrAddValueRef(node) = new List<MonoEdge>();
            }
        }

        void InitChildEdges()
        {
            var edges = GetComponentsInChildren<MonoEdge>();
            foreach (var edge in edges)
            {
                AddEdge(edge);
            }
        }

        public bool Contains(MonoNode node)
            => _dictionary.ContainsKey(node);

        public bool Contains(MonoEdge edge)
        {
            if (_dictionary.TryGetValue(edge.Source, out var list))
            {
                return list.Contains(edge);
            }

            return false;
        }

        public MonoEdge GetEdge(MonoNode source, MonoNode target, out bool found)
        {
            if (_dictionary.TryGetValue(source, out var list))
            {
                found = true;
                return list.Find(x => x.Source.Equals(target) || x.Target.Equals(source));
            }

            found = false;
            return null;
        }

        public IEnumerable<MonoEdge> GetEdges()
        {
            var hash = new HashSet<MonoEdge>();
            foreach (var pair in _dictionary)
            {
                foreach (var edge in pair.Value)
                {
                    hash.Add(edge);
                }
            }

            return hash;
        }

        public IEnumerable<MonoEdge> GetEdges(MonoNode node)
        {
            return _dictionary.TryGetValue(node, out var edges) ? edges : null;
        }

        public IEnumerable<MonoNode> GetNodes()
            => _dictionary.Select(x => x.Key);

        public MonoNode GetNodeByID(int id)
            => _dictionary.FirstOrDefault(x => x.Key.ID == id).Key;

        public bool AcceptDuplicateEdges => false;

        public bool AddEdge(MonoEdge edge)
            => AddEdge(edge, false);

        public bool AddEdge(MonoEdge edge, bool @override)
        {
            ref var s = ref _dictionary.GetOrAddValueRef(edge.Source);
            AddEdgeInner(edge, @override, s, true);
            ref var t = ref _dictionary.GetOrAddValueRef(edge.Target);
            AddEdgeInner(edge, @override, t, false);
            return true;
        }

        private void AddEdgeInner(MonoEdge edge, bool @override, List<MonoEdge> s, bool source)
        {
            if (s == null)
            {
                if (edge.Source.ID == 0)
                {
                    Debug.LogWarning($"No ID Edge Node {edge.Source}");
                    edge.Source.SetID(++idCounter);
                }

                s = new List<MonoEdge>();
            }
            else
            {
                var t = source ? edge.Source : edge.Target;
                var ot = (source ? edge.Target : edge.Source);
                var oldEdge = s.Find(x => (ot == x.Source && t == x.Target) || (ot == x.Target && t == x.Source));
                if (oldEdge != null)
                {
                    if (oldEdge.Equals(edge))
                    {
                        Debug.LogWarning($"Already Added {edge}");
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate Edge {oldEdge} {edge}");
                        if (@override)
                        {
                            s.Add(edge);
                        }
                    }
                }
                else
                    s.Add(edge);
            }
        }

        public bool RemoveEdge(MonoEdge edge)
        {
            bool x, y;
            x = y = false;
            if (_dictionary.TryGetValue(edge.Source, out var listS))
            {
                x = listS.Remove(edge);
            }

            if (_dictionary.TryGetValue(edge.Target, out var listT))
            {
                y = listT.Remove(edge);
            }

            return x & y;
        }
        
        public bool RemoveNode(MonoNode node)
        {
            if (!_dictionary.TryGetValue(node, out var list)) return false;
            for (var i = 0; i < list.Count; i++)
            {
                var edge = list[i];
                var ot = edge.GetOther(node);
                if (_dictionary.TryGetValue(ot, out var otList))
                {
                    otList.Remove(edge);
                }

                Destroy(edge);
            }
            list.Clear();
            _dictionary.Remove(node);
            Destroy(node);
            return true;
        }
        
        private void OnDestroy()
        {
            _dictionary = null;
        }

        public float Calc(MonoEdge value)
            => Vector3.Distance(value.Source.transform.position, value.Target.transform.position);

        public void ResetNodeColors()
        {
            foreach (var node in GetNodes())
            {
                node.Mark(Color.white);
            }
        }
        
        public void ResetEdgeColors()
        {
            foreach (var edge in GetEdges())
            {
                edge.Renderer.startColor = edgeSourceDefault;
                edge.Renderer.endColor = edgeTargetDefault;
            }
        }
    }
}