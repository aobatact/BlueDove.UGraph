using System;
using BlueDove.UGraph;
using UnityEngine;
using UnityEngine.Serialization;

namespace BlueDove.Sample
{
    public class GraphObjectSelector<TNode, TEdge> where TNode : MonoBehaviour, IEquatable<TNode> where TEdge : MonoBehaviour, IEdge<TNode>, IEquatable<TEdge>
    {
        public GraphObjectSelector(MultiSelectRay selectRay)
        {
            this.selectRay = selectRay;
        }

        private readonly MultiSelectRay selectRay;
        public event Action<TNode> NodeAction;
        public event Action<TEdge> EdgeAction;
        
        private void OnSelect(RaycastHit[] raycastHits, int count)
        {
            TEdge firstEdge = null;
            for (int i = 0; i < count; i++)
            {
                var hit = raycastHits[i];
                var obj = hit.transform.gameObject;
                var node = obj.GetComponent<TNode>();
                if (node != null)
                {
                    NodeAction?.Invoke(node);
                    return;
                }
                if (!(firstEdge is null)) continue;
                var edge = obj.GetComponent<TEdge>();
                if (edge != null)
                {
                    firstEdge = edge;
                }
            }
            if (!(firstEdge is null))
            {
                EdgeAction?.Invoke(firstEdge);
            }
        }
    }
}
