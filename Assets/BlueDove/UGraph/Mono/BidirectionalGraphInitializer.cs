using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace BlueDove.UGraph.Mono
{
    public abstract class BidirectionalGraphInitializer<TNode, TEdge, TGraph> : MonoBehaviour
        where TNode : NodeBase, IEquatable<TNode>
        where TEdge : EdgeBase<TNode>, IEquatable<TEdge>
        where TGraph : BidirectionalGraphBase<TNode, TEdge>
    {
        public TGraph Graph;
        public bool InitNodeFromChild;

        public bool InitRandomNodes => !count.Equals(default);
        public float2 size;
        public int2 count;
        public uint seed = 1234;
        public TNode nodePrefab;

        public bool InitEdgeFromChild;
        
        public TEdge edgePrefab;
        public float maxDistSq;
        public float minAngle;

        public bool DestroyAfterInit;

        private void Init()
        {
            Graph.Init();
            TNode[] nodes;
            if (InitNodeFromChild)
            {
                nodes = Graph.GetComponentsInChildren<TNode>();
            }
            else if (InitRandomNodes)
            {
                nodes = GraphUtils.GeneratePoints(size, count, x =>
                {
                    var n = Instantiate(nodePrefab, Unsafe.As<float3, Vector3>(ref x), Quaternion.identity, transform);
                    n.ID = Graph.IDPublisher.Publish();
                    return n;
                }, seed);
            }
            else
            {
                return;
            }
            foreach (var node in nodes)
                Graph.AddNode(node);

            if (InitEdgeFromChild)
                foreach (var edge in Graph.GetComponentsInChildren<TEdge>())
                {
                    Graph.AddEdge(edge);
                }
            else
            {
                GraphUtils.CreateEdges(Graph, nodes, maxDistSq, minAngle, (x, y) =>
                {
                    var e = Instantiate(edgePrefab, transform);
                    e.Source = x;
                    e.Target = y;
                    return new DirectionalEdge<TNode, TEdge>(e);
                });
            }

            if (DestroyAfterInit)
            {
                Destroy(this);
            }
        }

        private void OnEnable() => Init();
    }
}
