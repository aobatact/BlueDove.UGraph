using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BlueDove.UCollections;
using BlueDove.UCollections.Native;
using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using Unity.Collections;
using UnityEngine;
using BiEdge = BlueDove.UGraph.DirectionalEdge<BlueDove.Sample.MonoNode, BlueDove.Sample.MonoEdge>;
namespace BlueDove.Sample
{
    public class PathSearcher : MonoBehaviour
    {
        public MonoNode StartPoint;
        public MonoNode EndPoint;
        public MonoGraph Graph;
        public bool ChangeStart;
        public ImmutableList<BiEdge> CurrentPath; 
        
        private void Start()
        {
            ChangeStart = true;
            if (Graph == null)
            {
                Debug.LogAssertion("Graph is null");
            }
        }
        
        private void ResetColorOnPath()
        {
            Graph.ResetNodeColors();
            Graph.ResetEdgeColors();
            SetColorOnPath();
        }

        private void OnEnable()
        {
            var selector = GetComponent<SelectRay>();
            if (selector != null)
                selector.HitAction += ChangeTarget;
        }

        private void OnDisable()
        {
            var selector = GetComponent<SelectRay>();
            if (selector != null)
                selector.HitAction -= ChangeTarget;
        }

        ImmutableList<BiEdge> SearchNodes()
        {
            if(StartPoint == null || EndPoint == null) return ImmutableList<BiEdge>.Empty;
            using (var heap =
                new NativeRadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>(Allocator.Persistent))
            {
                //var heap = new RadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>();
                var immutableList = AStarAlgorithm
                    .Compute<MonoNode, BiEdge, MonoGraph,
                        NativeRadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>,
                        MonoGraph, MonoNode>(Graph, heap, Graph, StartPoint, EndPoint);
                return immutableList;
            }
        }

        void SetColorOnPath()
        {
            var immutableList = SearchNodes();
            if (immutableList.IsEmpty)
            {
                if(StartPoint.Equals(EndPoint))
                    Debug.Log("Start == End");
                else
                    Debug.LogWarning("Root Not Found");
            }
            else
            {
                CurrentPath = immutableList;
                foreach (var edge in immutableList)
                {
                    edge.Edge.Renderer.startColor = Color.green;
                    edge.Edge.Renderer.endColor = Color.cyan;
                }
            }
        }

        void ChangeTarget(RaycastHit raycastHit)
        {
            var node = raycastHit.transform.GetComponent<MonoNode>();
            if (node != null)
            {
                if (ChangeStart)
                {
                    ChangeStart = false;
                    StartPoint = node;
                    if (node is IMarkable m)
                    {
                        m.Mark(Color.magenta);
                    }
                }
                else
                {
                    ChangeStart = true;
                    EndPoint = node;
                    ResetColorOnPath();
                }
            }
            else
            {
                var edge = raycastHit.transform.GetComponent<MonoEdge>();
                if (edge != null)
                {
                    Debug.Log($"Selected Edge : {edge.name}");
                }
            }
        }
    }
}
