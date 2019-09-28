using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BlueDove.UGraph
{
    public interface IEdge<out TNode>
    {
        TNode Source { get; }
        TNode Target { get; }
    }

    public static class EdgeEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TNode GetOther<TNode, TNodeLike, TEdge>(this TEdge edge, TNodeLike node)
            where TNodeLike : IEquatable<TNode>
            where TEdge : IEdge<TNode>
        {
            if (node.Equals(edge.Source))
                return edge.Target;
            else
            {
                Debug.Assert(node.Equals(edge.Target));
                return edge.Source;
            }
        }

        public static TNode GetOther<TNode, TEdge>(this TEdge edge, TNode node)
            where TNode : IEquatable<TNode> where TEdge : IEdge<TNode>
                => GetOther<TNode, TNode, TEdge>(edge, node);
    }

    /// <summary>
    /// Mark Node for Debug
    /// </summary>
    public interface IMarkable
    {
        void Mark(Color color);
    }

}
