using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BlueDove.UCollections;

namespace BlueDove.UGraph.Algorithm
{
    public static class IDAStarAlgorithm
    {
        public static TEdge[] Compute<TNode, TEdge, TGraph, TGFunc, TEndNode>(
            TGraph graph, TGFunc costFunc, TNode start, TEndNode end)
            where TNode : IEquatable<TNode>, IIDHolder
            where TEdge : IEdge<TNode>
            where TGraph : IGraph<TNode, TEdge>
            where TGFunc : ICostFunc<TEdge>
            where TEndNode : IEquatable<TNode>, ICostFunc<TNode>
        {
            var bound = end.Calc(start);
            var pathStack = new StackLite<(TEdge edge, float g, float min, IEnumerator<TEdge> enumerator)>();
            try
            {
                L0:
                var head = start;
                var g = 0f;
                L1:
                if (end.Equals(head))
                {
                    var res = new TEdge[pathStack.Count];
                    for (var i = 0; i < pathStack.Values.Length; i++)
                    {
                        res[i] = pathStack.Values[i].edge;
                    }
                    return res;
                }
                var en = graph.GetEdges(head).GetEnumerator();
                var nextF = float.PositiveInfinity;
                L2:
                if (en.MoveNext())
                {
                    var c = en.Current;
                    var nG = g + costFunc.Calc(c);
                    var nF = nG + end.Calc(c.Target);
                    if (nF < nextF) nextF = nF;
                    if (nF >= bound) goto L2;
                    pathStack.Push((c, g, nextF, en));
                    head = c.Target;
                    goto L1;
                }
                en?.Dispose();
                if (pathStack.Count > 0)
                {
                    TEdge nEdge;
                    float nNf;
                    (nEdge, g, nNf, en) = pathStack.Pop();
                    head = nEdge.Target;
                    if (nNf < nextF) nextF = nNf;
                    goto L2;
                }
                if (!float.IsPositiveInfinity(nextF))
                {
                    bound = nextF;
                    goto L0;
                }
                return null;
            }
            finally
            {
                while (pathStack.Count > 0)
                {
                    pathStack.Pop().enumerator?.Dispose();
                }
            }
        }
    }
}
