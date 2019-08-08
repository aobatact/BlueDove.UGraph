using System.Collections.Generic;

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
}