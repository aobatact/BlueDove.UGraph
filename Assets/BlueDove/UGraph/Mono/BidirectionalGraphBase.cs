using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueDove.UGraph.Mono
{
    public abstract class BidirectionalGraphBase<TNode, TEdge> : MonoBehaviour, IGraph<TNode, DirectionalEdge<TNode, TEdge>>, 
        IWritableGraph<TNode, DirectionalEdge<TNode, TEdge>>
        where TNode : NodeBase, IEquatable<TNode>
        where TEdge : EdgeBase<TNode>, IEquatable<TEdge>
    {
        //private BidirectionalGraph<TNode, DirectionalEdge<TNode, TEdge>> _graph;
        private BidirectionalGraph<TNode, TEdge> _graph;
        public IDPublisherS IDPublisher;

        // Start is called before the first frame update
        public void Init()
        {
            _graph = new BidirectionalGraph<TNode, TEdge>(4);
            IDPublisher = new IDPublisherS();
        }

        private void Start()
        {
            if (!_graph.IsInit)
            {
                Init();
            }
        }

        protected void OnDestroy() 
            => _graph = default;

        #region IGraph

        public bool Contains(TNode node) => _graph.Contains(node);

        public bool Contains(DirectionalEdge<TNode, TEdge> edge) => _graph.Contains(edge);

        public DirectionalEdge<TNode, TEdge> GetEdge(TNode source, TNode target, out bool found)
            => _graph.GetEdge(source, target, out found);

        public IEnumerable<DirectionalEdge<TNode, TEdge>> GetEdges() => _graph.GetEdges();

        public IEnumerable<DirectionalEdge<TNode, TEdge>> GetEdges(TNode node) => _graph.GetEdges(node);

        public IEnumerable<TNode> GetNodes() => _graph.GetNodes();

        public bool AcceptDuplicateEdges => _graph.AcceptDuplicateEdges;

        public bool AddNode(TNode node)
        {
            if (node.ID == 0)
            {
                node.ID = IDPublisher.Publish();
            }
            return _graph.AddNode(node);
        }

        public void AddEdge(TEdge edge) => AddEdge(new DirectionalEdge<TNode, TEdge>(edge));

        public bool AddEdge(DirectionalEdge<TNode, TEdge> edge)
        {
            if (edge.Source.ID == 0)
            {
                edge.Source.ID = IDPublisher.Publish();
            }
            if (edge.Target.ID == 0)
            {
                edge.Target.ID = IDPublisher.Publish();
            }
            return _graph.AddEdge(edge);
        }

        public bool RemoveNode(TNode node) => _graph.RemoveNode(node);

        public bool RemoveEdge(DirectionalEdge<TNode, TEdge> edge) => _graph.RemoveEdge(edge);

        public void Clear() => _graph.Clear();

        #endregion
    }
}
