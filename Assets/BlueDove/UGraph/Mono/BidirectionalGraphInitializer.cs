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
        public TGraph graph;
        public bool initNodeFromChild;
        public Transform nodeRootTransform;
        public Transform edgeRootTransform;
        
        public bool InitRandomNodes => !count.Equals(default);
        public float2 size;
        public int2 count;
        public uint seed = 1234;
        public TNode nodePrefab;

        public bool initEdgeFromChild;
        
        public TEdge edgePrefab;
        public float maxDistSq;
        public float minAngle;

        public bool destroyAfterInit;

        private void Init()
        {
            graph.Init();
            TNode[] nodes;
            if (initNodeFromChild)
            {
                nodes = graph.GetComponentsInChildren<TNode>();
            }
            else if (InitRandomNodes)
            {
                if (nodeRootTransform is null)
                {
                    nodeRootTransform = graph.transform;
                }
                nodes = GraphUtils.GeneratePoints(size, count, x =>
                {
                    var n = Instantiate(nodePrefab, Unsafe.As<float3, Vector3>(ref x), Quaternion.identity, nodeRootTransform);
                    n.ID = graph.idPublisher.Publish();
                    return n;
                }, seed);
            }
            else
            {
                return;
            }
            foreach (var node in nodes)
                graph.AddNode(node);

            if (initEdgeFromChild)
                foreach (var edge in graph.GetComponentsInChildren<TEdge>())
                {
                    graph.AddEdge(edge);
                }
            else
            {
                if (edgeRootTransform is null)
                {
                    edgeRootTransform = graph.transform;
                }
                GraphUtils.CreateEdges(graph, nodes, maxDistSq, minAngle, (x, y) =>
                {
                    var e = Instantiate(edgePrefab, edgeRootTransform);
                    e.Source = x;
                    e.Target = y;
                    return new DirectionalEdge<TNode, TEdge>(e);
                });
            }

            if (destroyAfterInit)
            {
                Destroy(this);
            }
        }

        private void OnEnable() => Init();
    }
}
