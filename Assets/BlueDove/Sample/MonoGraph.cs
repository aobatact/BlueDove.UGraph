using System;
using System.Collections.Generic;
using System.Linq;
using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using Microsoft.Collections.Extensions;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Serialization;

namespace BlueDove.Sample
{
    public class MonoGraph : MonoBehaviour, IGraph<MonoNode, MonoEdge>, ICostFunc<MonoEdge>
    {
        private int idCounter;
        [FormerlySerializedAs("prefab")] [SerializeField]
        private MonoNode nodePrefab;
        [SerializeField]
        private MonoEdge edgePrefab;
        private DictionarySlim<MonoNode, List<MonoEdge>> _dictionary;
        
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
        
        // Start is called before the first frame update
        void Start()
        {
            _dictionary = new DictionarySlim<MonoNode, List<MonoEdge>>();
            idCounter = 0;
            InitChildNodes();
            InitChildEdges();
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
        {
            ref var s = ref _dictionary.GetOrAddValueRef(edge.Source);
            if (s == null)
            {
                if(edge.Source.ID == 0)
                {
                    Debug.LogWarning("No ID Edge Source");
                    edge.Source.SetID(++idCounter);
                }
                s = new List<MonoEdge>();
            }
            s.Add(edge);
            ref var t = ref _dictionary.GetOrAddValueRef(edge.Target);
            if (t == null)
            {
                if (edge.Target.ID == 0)
                {
                    Debug.LogWarning("No ID Edge Target");
                    edge.Target.SetID(++idCounter);   
                }
                t = new List<MonoEdge>();
            }
            t.Add(edge);
            return true;
        }

        private void OnDestroy()
        {
            _dictionary = null;
        }

        public float Calc(MonoEdge value) 
            => Vector3.Distance(value.Source.transform.position, value.Target.transform.position);
    }
}
