using UnityEngine;

namespace BlueDove.UGraph.Mono
{
    public class NodeCursorBase<TNode, TEdge, TGraph> : MonoBehaviour
        where TNode : IVector3Node
        where TEdge : IEdge<TNode>
        where TGraph : IGraph<TNode, TEdge>
    {
        [SerializeField] private TGraph graph;
        private TNode _currentNode;
        private Camera _camera;
        
        TNode GetClosestNode() 
            => GraphUtils.ClosestNode<TNode, TEdge, TGraph>(graph, transform.position);

        private void OnEnable()
        {
            if(_currentNode == null)
                _currentNode = GetClosestNode();
        }

        TNode NextNode(Vector2 direction)
        {
            var dir = (Vector3)direction.normalized;
            var iProduct = -2f;
            var next = default(TNode);
            foreach (var edge in graph.GetEdges(_currentNode))
            {
                var vec =  edge.GetDirectionalVector<TNode, TEdge>();
                var newProduct = Vector3.Dot(dir, vec);
                var xProduct = Mathf.Sqrt(newProduct * newProduct / vec.sqrMagnitude);
                if (xProduct > iProduct)
                {
                    iProduct = xProduct;
                    next = edge.Target;
                }
            }

            return next;
        }
    }
}
