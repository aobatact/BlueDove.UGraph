using UnityEngine;

namespace BlueDove.UGraph
{
    public interface IEdge<out TNode>
    {
        /// <summary>
        /// Source of Edge
        /// </summary>
        TNode Source { get; }
        /// <summary>
        /// Target of Edge
        /// </summary>
        TNode Target { get; }
    }

    /// <summary>
    /// Mark Node for Debug
    /// </summary>
    public interface IMarkable
    {
        void Mark(Color color);
    }

}
