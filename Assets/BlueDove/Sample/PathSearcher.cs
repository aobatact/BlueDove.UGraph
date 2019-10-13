using System.Collections.Generic;
using System.Collections.Immutable;
using BlueDove.Sample;
using BlueDove.UCollections;
using BlueDove.UCollections.Native;
using BlueDove.UGraph.Algorithm;
using Unity.Collections;
using UnityEngine;

public class PathSearcher : MonoBehaviour
{

    public MonoNode StartPoint;
    public MonoNode EndPoint;
    public MonoGraph Graph;
    public bool SearchNextFrame;

    private void Start()
    {
        SearchNextFrame = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (SearchNextFrame)
        {
            SearchNextFrame = false;
            Graph.ResetNodeColors();
            Graph.ResetEdgeColors();
            SetColorOnPath();
        }
    }

    ImmutableList<MonoEdge> SearchNodes()
    {
        var heap = new NativeRadixHeap<KeyValuePair<float,int>,FloatIntValueConverter>(Allocator.Persistent);
        //var heap = new RadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>();
        var immutableList = AStarAlgorithm
            .Compute<MonoNode, MonoEdge, MonoGraph, NativeRadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>,
                MonoGraph, MonoNode>(Graph,heap, Graph, StartPoint, EndPoint);
        heap.Dispose();
        return immutableList;
    }

    void SetColorOnPath()
    {
        var immutableList = SearchNodes();
        if (immutableList.IsEmpty)
        {
            Debug.LogWarning("Root Not Found");
        }
        else
        {
            foreach (var edge in immutableList)
            {
                edge.Renderer.startColor = Color.green;
                edge.Renderer.endColor = Color.cyan;
            }
        }
    }
}
