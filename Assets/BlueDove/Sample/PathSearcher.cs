using System.Collections.Generic;
using System.Collections.Immutable;
using BlueDove.Collections.Heaps;
using BlueDove.Sample;
using BlueDove.UGraph.Algorithm;
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
        return AStarAlgorithm
            .Compute<MonoNode, MonoEdge, MonoGraph, RadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>,
                MonoGraph, MonoNode>(Graph,
                new RadixHeap<KeyValuePair<float, int>, FloatIntValueConverter>(), Graph, StartPoint, EndPoint);
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
