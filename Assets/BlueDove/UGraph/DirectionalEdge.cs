using System;

namespace BlueDove.UGraph
{
    /// <summary>
    /// Wrapper of <see cref="TEdge"/> with direction
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
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

        /// <summary>
        /// Source of the Edge. This is Reversed from the base Edge if <see cref="Direction"> is false.
        /// </summary>
        public TNode Source => Direction ? Edge.Source : Edge.Target;

        /// <summary>
        /// Target of the Edge. This is Reversed from the base Edge if <see cref="Direction"> is false.
        /// </summary>
        public TNode Target => Direction ? Edge.Target : Edge.Source;

        public DirectionalEdge<TNode, TEdge> Reverse() => new DirectionalEdge<TNode, TEdge>(Edge, !Direction);

        public bool Equals(DirectionalEdge<TNode, TEdge> other)
            => Direction.Equals(other.Direction) && Edge.Equals(other.Edge);
    }
}