using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using BlueDove.UGraph.Mono;
using UnityEngine;

namespace BlueDove.SampleV2
{
    public class SampleGraph : BidirectionalGraphBase<SampleNode, SampleEdge>,
        ICostFunc<DirectionalEdge<SampleNode, SampleEdge>>
    {
        public Color defaultNodeColor;
        public Color defaultEdgeColor;

        public void ResetNodeColors()
        {
            foreach (var node in GetNodes())
            {
                node.Mark(defaultNodeColor);
            }
        }

        public void ResetEdgeColors()
        {
            foreach (var edge in GetEdges())
            {
                if (edge.Direction)
                {
                    edge.Edge.SetColor(defaultEdgeColor);
                }
            }
        }

        public float Calc(DirectionalEdge<SampleNode, SampleEdge> value) => value.Edge.Direction.magnitude;
    }
}