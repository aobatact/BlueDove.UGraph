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
        private IDPublisherS _idPublisher;
        [SerializeField] private MonoNode nodePrefab;
        [SerializeField] private MonoEdge edgePrefab;
        private DictionarySlim<MonoNode, List<MonoEdge>> _dictionary;
        [SerializeField] private bool autoCreateNodes;
        [SerializeField] private bool autoCreateEdges;
        [SerializeField] private float minDistSq;
        [SerializeField] private float minAngle;
        [SerializeField] private Color edgeSourceDefault;
        [SerializeField] private Color edgeTargetDefault;

        public MonoNode CreateNewNode(Vector3 pos)
        {
            var node = Instantiate(nodePrefab, pos, Quaternion.identity);
            node.SetID(_idPublisher.Publish());
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

        // Start is called before the first frame update
        void Start()
        {
            _dictionary = new DictionarySlim<MonoNode, List<MonoEdge>>();
            _idPublisher = default(IDPublisherS);
            InitChildNodes();
            if (autoCreateEdges)
            {
                GraphUtils.CreateEdges<MonoNode, MonoEdge, MonoGraph>(this, minDistSq, minAngle, (x, y) =>
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
                node.SetID(_idPublisher.Publish());
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

        public bool AddNode(MonoNode node)
        {            
            ref var list = ref _dictionary.GetOrAddValueRef(node);
            if (list == null)
            {
                list = new List<MonoEdge>();
                return true;
            }
            return false;
        }

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
                    edge.Source.SetID(_idPublisher.Publish());
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