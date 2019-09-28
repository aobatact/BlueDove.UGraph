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
        bool RemoveEdge(TEdge edge);
    }
    
    public interface IReadOnlyNativeGraph<TNode, TEdge> where TNode : struct where TEdge : struct, IEdge<TNode>
    {
        bool Contains(TNode node);
        bool Contains(TEdge edge);
        TEdge GetEdge(TNode source, TNode target, out bool found);
        /// <summary>
        /// Require to free NativeArray of GetEdges & GetNodes
        /// </summary>
        bool RequireFreeArray { get; }
        NativeArray<TEdge> GetEdges(Allocator allocator);
        NativeArray<TEdge> GetEdges(TNode node, Allocator allocator);
        NativeArray<TEdge> GetEdges(TNode source, TNode target, Allocator allocator);
        NativeArray<TNode> GetNodes(Allocator allocator);
    }

    public readonly struct ReadOnlyNativeGraphWrapper<TNode, TEdge, TGraph> : IReadOnlyGraph<TNode, TEdge>
        where TNode : struct
        where TEdge : struct, IEdge<TNode>
        where TGraph : IReadOnlyNativeGraph<TNode, TEdge>
    {
        private readonly TGraph _graph;
        private readonly Allocator _allocator;

        public ReadOnlyNativeGraphWrapper(TGraph graph, Allocator allocator)
        {
            _graph = graph;
            _allocator = allocator;
        }

        public bool Contains(TNode node) => _graph.Contains(node);

        public bool Contains(TEdge edge) => _graph.Contains(edge);

        public TEdge GetEdge(TNode source, TNode target, out bool found)
            => _graph.GetEdge(source, target, out found);

        public IEnumerable<TEdge> GetEdges() => _graph.GetEdges(_allocator);

        public IEnumerable<TEdge> GetEdges(TNode node) => _graph.GetEdges(node,_allocator);

        public IEnumerable<TNode> GetNodes() => _graph.GetNodes(_allocator);
    }
    
    public class SimpleDictionaryGraph<TNode, TEdge> : IGraph<TNode, TEdge> where TEdge : IEdge<TNode> where TNode : IEquatable<TNode>
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

        public bool RemoveEdge(TEdge edge)
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
    }
}