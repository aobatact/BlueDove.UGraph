using System.Collections.Generic;
using BlueDove.UCollections;
using Unity.Collections;

namespace BlueDove.UGraph
{
    public readonly struct ReadOnlyNativeGraphWrapper<TNode, TEdge, TGraph> : IGraph<TNode, TEdge>
        where TNode : struct
        where TEdge : struct, IEdge<TNode>
        where TGraph : INativeGraph<TNode, TEdge>
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

        public IEnumerable<TEdge> GetEdges() =>
            new NativeArrayEnumerableWrapper<TEdge>(_graph.GetEdges(_allocator), _graph.RequireFreeArray);

        public IEnumerable<TEdge> GetEdges(TNode node) =>
            new NativeArrayEnumerableWrapper<TEdge>(_graph.GetEdges(node, _allocator), _graph.RequireFreeArray);

        public IEnumerable<TNode> GetNodes() =>
            new NativeArrayEnumerableWrapper<TNode>(_graph.GetNodes(_allocator), _graph.RequireFreeArray);
    }
}