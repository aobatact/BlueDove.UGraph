using System.Collections.Generic;

namespace BlueDove.UGraph
{
    public interface IGraph<TNode, TEdge> : IGraph<TNode,TEdge,IEnumerable<TEdge,IEnumerator<TEdge>>,IEnumerator<TEdge>> where TEdge : IEdge<TNode>
    { }

    public interface IGraph<TNode, TEdge, TEdgeEnumerable, TEdgeEnumerator> where TEdge : IEdge<TNode>
        where TEdgeEnumerable: IEnumerable<TEdge,TEdgeEnumerator> where TEdgeEnumerator : IEnumerator<TEdge>
    {
        bool Contains(TNode node);
        bool Contains(TEdge node);
        TEdge GetEdge(TNode source, TNode target);
        TEdgeEnumerable GetEdges(TNode node);
        IEnumerable<TNode> GetNodes();
    }

    public interface IEnumerable<T, TEnumerator>
        where TEnumerator : IEnumerator<T>
    {
        TEnumerator GetEnumerator();
    }
}