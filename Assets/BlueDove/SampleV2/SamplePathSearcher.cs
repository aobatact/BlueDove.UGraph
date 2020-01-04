using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
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
        private SampleGraph graph;

        public Color pathColor;

        private void OnEnable()
        {
            var ray = GetComponent<SelectRay>();
            if (ray != null)
            {
                ray.HitsAction += OnHits;
            }
        }

        private void OnDisable()
        {
            var ray = GetComponent<SelectRay>();
            if (ray != null)
            {
                ray.HitsAction -= OnHits;
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
                        graph, heap, graph, startNode, endNode);
            }
        }

        public void PaintPath()
        {
            var path = GetPath();
            if (path.IsEmpty) return;
            foreach (var edge in path)
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

        public void OnHits(RaycastHit[] hits, int count)
        {
            var node = ObjSelector.GetObjectFromHits<SampleNode>(hits, count);
            if (!(node is null)) SelectNode(node);
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
                    graph.ResetNodeColors();
                    graph.ResetEdgeColors();
                    PaintPath();
                    startNode = null;
                    endNode = null;
                }
            }
        }
    }
}
