using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Collections.Extensions;
using Unity.Collections;

namespace BlueDove.UGraph
{
    public interface IReadOnlyGraph<TNode, TEdge> where TEdge : IEdge<TNode>
    {
        bool Contains(TNode node);
        bool Contains(TEdge edge);
        TEdge GetEdge(TNode source, TNode target, out bool found);
        IEnumerable<TEdge> GetEdges();
        IEnumerable<TEdge> GetEdges(TNode node);
        IEnumerable<TNode> GetNodes();
    }

    public interface IGraph<TNode, TEdge> : IReadOnlyGraph<TNode, TEdge> where TEdge : IEdge<TNode>
    {
        bool AcceptDuplicateEdges { get; }
        bool AddEdge(TEdge edge);
    }
    
    public interface IReadOnlyNativeGraph<TNode, TEdge> where TNode : struct where TEdge : struct, IEdge<TNode>
    {
        bool Contains(TNode node);
        bool Contains(TEdge edge);
        TEdge GetEdge(TNode source, TNode target, out bool found);
        NativeArray<TEdge> GetEdges();
        NativeArray<TEdge> GetEdges(TNode node);
        NativeArray<TNode> GetNodes();
    }

    public class SimpleDictionaryGraph<TNode, TEdge> : IGraph<TNode, TEdge> where TEdge : IEdge<TNode> where TNode : IEquatable<TNode>
    {
        private DictionarySlim<TNode, List<TEdge>> _dictionary;

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
                    if (edge.Source.Equals(target) || edge.Target.Equals(target))
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
        {
            var hash = new HashSet<TEdge>();
            foreach (var pair in _dictionary)
                for (var i = 0; i < pair.Value.Count; i++)
                    hash.Add(pair.Value[i]);
            return hash;
        }

        public IEnumerable<TEdge> GetEdges(TNode node)
            => _dictionary.TryGetValue(node, out var edges) ? edges : (IEnumerable<TEdge>) Array.Empty<TEdge>();

        public IEnumerable<TNode> GetNodes() => _dictionary.Select(x => x.Key);
        public bool AcceptDuplicateEdges => false;

        public bool AddEdge(TEdge edge)
        {
            ref var edgesS = ref _dictionary.GetOrAddValueRef(edge.Source);
            if (edgesS == null)
                edgesS = new List<TEdge>();
            else if (edgesS.Contains(edge))
                return false;
            edgesS.Add(edge);
            ref var edgesT = ref _dictionary.GetOrAddValueRef(edge.Target);
            if (edgesT == null)
            {
                edgesT = new List<TEdge>();
            }
            edgesT.Add(edge);
            return true;
        }
    }
}