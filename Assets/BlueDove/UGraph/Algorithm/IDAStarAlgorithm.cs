using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BlueDove.UCollections;

namespace BlueDove.UGraph.Algorithm
{
    public static class IDAStarAlgorithm
    {
        public static IEnumerable<TEdge> Compute<TNode, TEdge, TGraph, THeap, TGFunc, TEndNode>(
            TGraph graph, TGFunc costFunc, TNode start, TEndNode end)
            where TNode : IEquatable<TNode>, IIDHolder
            where TEdge : IEdge<TNode>
            where TGraph : IGraph<TNode, TEdge>
            where TGFunc : ICostFunc<TEdge>
            where TEndNode : IEquatable<TNode>, ICostFunc<TNode>
        {
            var bound = end.Calc(start);
            var stack = new Stack<TEdge>();
            var head = start;
            while (true)
            {
                foreach (var edge in graph.GetEdges(start))
                {
                    
                }
                var res = ComputeInner(graph, costFunc, end, stack, head, 0, bound);
                if (res < 0)
                {
                    return stack.Reverse();
                }
                if (float.IsInfinity(res))
                {
                    return null;
                }
                bound = res;
            }
        }

        public static float ComputeInner<TNode, TEdge, TGraph, TGFunc, TEndNode>(
            TGraph graph, TGFunc costFunc, TEndNode end, Stack<TEdge> stack, TNode head, float g, float bound)
            where TNode : IEquatable<TNode>, IIDHolder
            where TEdge : IEdge<TNode>
            where TGraph : IGraph<TNode, TEdge>
            where TGFunc : ICostFunc<TEdge>
            where TEndNode : IEquatable<TNode>, ICostFunc<TNode>
        {
            if (end.Equals(head))
                return -1;
            var f = g + end.Calc(head);
            if (f > bound) return f;
            var min = float.PositiveInfinity; //float.PositiveInfinity;
            foreach (var edge in graph.GetEdges(head))
            {
                if (!stack.Contains(edge))
                {
                    stack.Push(edge);
                    var nF = ComputeInner(graph, costFunc, end, stack,
                        edge.Target, g + costFunc.Calc(edge), bound);
                    if (nF < 0)
                        return nF;
                    if (nF < min) min = nF;
                    stack.Pop();
                }
            }
            return min;
        }
        
        struct SearchNode<TNode, TEdge>
        {
            public TNode Value;

            public SearchNode(TNode start)
            {
                Value = start;
            }
        }
    }
}
