using UnityEngine;

namespace BlueDove.UGraph
{
    public interface IEdge<out TNode>
    {
        TNode Source { get; }
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
