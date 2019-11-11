using System;

namespace BlueDove.UGraph
{
    public readonly struct DirectionalEdge<TNode, TEdge> : IEdge<TNode>, IEquatable<DirectionalEdge<TNode, TEdge>>
        where TEdge : IEdge<TNode>, IEquatable<TEdge>
    {
        public DirectionalEdge(TEdge edge, bool direction = true)
        {
            Edge = edge;
            Direction = direction;
        }

        public TEdge Edge { get; }
        public bool Direction { get; }

        public TNode Source => Direction ? Edge.Source : Edge.Target;
        public TNode Target => Direction ? Edge.Target : Edge.Source;

        public DirectionalEdge<TNode, TEdge> Reverse() => new DirectionalEdge<TNode, TEdge>(Edge, !Direction);

        public bool Equals(DirectionalEdge<TNode, TEdge> other)
            => Direction.Equals(other.Direction) && Edge.Equals(other.Edge);
    }
}