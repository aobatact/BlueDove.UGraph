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
    private bool X;

    private void Start()
    {
        X = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (X)
        {
            X = false;
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
        foreach (var edge in SearchNodes())
        {
            edge.Renderer.startColor = Color.green;
            edge.Renderer.endColor = Color.cyan;
        }
    }
}
