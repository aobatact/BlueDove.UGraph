using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Collections.Extensions;

namespace BlueDove.UGraph
{
    public struct SimpleDictionaryGraph<TNode, TEdge> : IWritableGraph<TNode, TEdge>, IGraph<TNode, TEdge> where TEdge : IEdge<TNode> where TNode : IEquatable<TNode>, IIDHolder
    {
        private readonly DictionarySlim<TNode, List<TEdge>> _dictionary;

        public SimpleDictionaryGraph(DictionarySlim<TNode, List<TEdge>> dictionary) { _dictionary = dictionary; }

        public bool Contains(TNode node) 
            => _dictionary.ContainsKey(node);

        public bool Contains(TEdge edge)
        {
            GetEdge(edge.Source, edge.Target, out var res);
            return res;
        }

        public TEdge GetEdge(TNode source, TNode target, out bool found)
        {
            if (_dictionary.TryGetValue(source, out var edges))
            {
                foreach (var edge in edges)
                {
                    if (edge.Target.Equals(target))
                    {
                        found = true;
                        return edge;
                    }
                }
            }
            found = false;
            return default;
        }

        public IEnumerable<TEdge> GetEdges() 
            => _dictionary.SelectMany(pair => pair.Value.Where(edge => edge.Source.ID < edge.Target.ID));

        public IEnumerable<TEdge> GetEdges(TNode node)
            => _dictionary.TryGetValue(node, out var edges) ? edges : (IEnumerable<TEdge>) Array.Empty<TEdge>();

        public IEnumerable<TNode> GetNodes() => _dictionary.Select(x => x.Key);

        public bool AcceptDuplicateEdges => false;

        public bool AddNode(TNode node) 
        { 
            ref var list = ref _dictionary.GetOrAddValueRef(node);
            if (list == null)
            {
                list = new List<TEdge>();
                return true;
            }
            return false;
        }

        public bool AddEdge(TEdge edge)
        {
            ref var edgesS = ref _dictionary.GetOrAddValueRef(edge.Source);
            if (edgesS == null)
                edgesS = new List<TEdge>();
            else if (edgesS.Contains(edge))
                return false;
            edgesS.Add(edge);
            return true;
        }

        public bool RemoveNode(TNode node) 
            => _dictionary.Remove(node);

        public bool RemoveEdge(TEdge edge)
        {
            return _dictionary.TryGetValue(edge.Source, out var listS) && listS.Remove(edge);
        }

        public void Clear() => _dictionary.Clear();
    }
}