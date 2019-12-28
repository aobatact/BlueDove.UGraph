using System.Collections.Generic;
using System.Collections.Immutable;
using BlueDove.InputUtils;
using BlueDove.UCollections;
using BlueDove.UCollections.Native;
using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using Unity.Collections;
using UnityEngine;

namespace BlueDove.SampleV2
{
    [RequireComponent(typeof(SelectRay))]
    public class SamplePathSearcher : MonoBehaviour
    {
        private SampleNode startNode;
        private SampleNode endNode;

        [SerializeField]
        private SampleGraph Graph;

        public Color pathColor;

        private void OnEnable()
        {
            var ray = GetComponent<SelectRay>();
            if (ray != null)
            {
                ray.HitAction += Hit;
            }
        }

        private void OnDisable()
        {
            var ray = GetComponent<SelectRay>();
            if (ray != null)
            {
                ray.HitAction -= Hit;
            }
        }

        public ImmutableList<DirectionalEdge<SampleNode, SampleEdge>> GetPath()
        {
            using (var heap =
                new NativeRadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>(Allocator.Persistent))
            {
                return AStarAlgorithm
                    .Compute<SampleNode, DirectionalEdge<SampleNode, SampleEdge>, SampleGraph,
                        NativeRadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>, SampleGraph, SampleNode>(
                        Graph, heap, Graph, startNode, endNode);
            }
        }

        public void PaintPath()
        {
            foreach (var edge in GetPath())
            {
                edge.Edge.SetColor(pathColor);
            }
        }

        public void Hit(RaycastHit hit)
        {
            var node = hit.transform.GetComponent<SampleNode>();
            if (node != null)
            {
                SelectNode(node);
            }
        }

        public void SelectNode(SampleNode node)
        {
            if (startNode == null)
            {
                startNode = node;
                startNode.Mark(Color.magenta);
            }
            else
            {
                endNode = node;
                if (endNode != null)
                {
                    Graph.ResetNodeColors();
                    Graph.ResetEdgeColors();
                    PaintPath();
                    startNode = null;
                    endNode = null;
                }
            }
        }
    }
}
