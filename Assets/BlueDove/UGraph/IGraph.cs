using System.Collections.Generic;
using Unity.Collections;

namespace BlueDove.UGraph
{
    public interface IGraph<TNode, TEdge> where TEdge : IEdge<TNode>
    {
        bool Contains(TNode node);
        bool Contains(TEdge edge);
        TEdge GetEdge(TNode source, TNode target, out bool found);
        IEnumerable<TEdge> GetEdges();
        IEnumerable<TEdge> GetEdges(TNode node);
        IEnumerable<TNode> GetNodes();
    }

    public interface IEditableGraph<in TNode, in TEdge> where TEdge : IEdge<TNode>
    {
        bool AcceptDuplicateEdges { get; }
        bool AddNode(TNode node);
        bool AddEdge(TEdge edge);
        bool RemoveNode(TNode node);
        bool RemoveEdge(TEdge edge);
        void Clear();
    }
    
    public interface INativeGraph<TNode, TEdge> where TNode : struct where TEdge : struct, IEdge<TNode>
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
}