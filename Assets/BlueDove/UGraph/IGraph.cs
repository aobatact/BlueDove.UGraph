using System.Collections.Generic;
using Unity.Collections;

namespace BlueDove.UGraph
{
    public interface IGraph<TNode, TEdge> where TEdge : IEdge<TNode>
    {
        bool Contains(TNode node);
        bool Contains(TEdge node);
        TEdge GetEdge(TNode source, TNode target);
        IEnumerable<TEdge> GetEdges(TNode node);
        IEnumerable<TNode> GetNodes();
    }
    
    public interface INativeGraph<TNode, TEdge> where TNode : struct where TEdge : struct, IEdge<TNode>
    {
        bool Contains(TNode node);
        bool Contains(TEdge node);
        TEdge GetEdge(TNode source, TNode target);
        NativeArray<TEdge> GetEdges(TNode node);
        NativeArray<TNode> GetNodes();
    }
}