using System.Collections.Generic;
using Unity.Collections;

namespace BlueDove.UGraph
{
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
}