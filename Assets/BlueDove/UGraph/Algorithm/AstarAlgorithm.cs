using System;
using System.Collections.Immutable;
using BlueDove.Collections.Heaps;
using Microsoft.Collections.Extensions;
namespace BlueDove.UGraph.Algorithm
{
    /// <summary>
    /// Class to calculate A*
    /// </summary>
    public static class AStarAlgorithm
    {
        /// <summary>
        /// Compute the A* algorithm
        /// </summary>
        /// <typeparam name="TNode">Node Type</typeparam>
        /// <typeparam name="TEdge">Edge Type</typeparam>
        /// <typeparam name="TGraph">Graph Type</typeparam>
        /// <typeparam name="THeap">Priority Queue to use inside</typeparam>
        /// <typeparam name="TGFunc">Function of calculating Cost in Edge</typeparam>
        /// <param name="graph">Graph to search</param>
        /// <param name="heap">Factory of creating heap</param>
        /// <param name="costFunc">Function of calculating Cost in Edge</param>
        /// <param name="start">Start Node.</param>
        /// <param name="end">Set of EndNode. This could be single node of some end condition.
        /// This should include function of calculating heuristic cost for Node
        /// </param>
        /// <typeparam name="TEndNode">Type to confirm the current node fulfill the end condition.</typeparam>
        /// <returns>Path to the start node to end node.</returns>
        public static ImmutableList<TEdge> Compute<TNode, TEdge, TGraph, THeap, TGFunc, TEndNode>(
            TGraph graph, THeap heap, TGFunc costFunc, TNode start, TEndNode end)
            where TNode : IEquatable<TNode>, IIDHolder
            where TEdge : IEdge<TNode>
            where TGraph : IReadOnlyGraph<TNode, TEdge>
            where THeap : IHeap<int>
            where TGFunc : ICostFunc<TEdge>
            where TEndNode : IEquatable<TNode>, ICostFunc<TNode>
        {
            var nodeList = new DictionarySlim<int, AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>>();
            TNode current;
            //If the start node fulfill the end condition, end the path finding.
            if (end.Equals(current = start))
                return ImmutableList<TEdge>.Empty;
            var min = new AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>(current);
            while (true)
            {
                Loop:
                //update cost connected to current  
                foreach (var edge in graph.GetEdges(current))
                {
                    var ot = edge.GetOther<TNode, TNode, TEdge>(current);
                    if(end.Equals(ot))
                        goto End;
                    ref var aNode = ref nodeList.GetOrAddValueRef(ot.ID);
                    if (aNode.ID == 0) aNode = new AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>(ot);
                    if (aNode.CurrentG <= min.CurrentG) continue;
                    var ng = min.CurrentG + costFunc.Calc(edge);
                    var nf = ng + end.Calc(ot);
                    if (aNode.Priority <= nf) continue;
                    aNode.Priority = nf;
                    aNode.CurrentG = ng;
                    aNode.Closed = false;
                    aNode.Path = min.Path.Add(edge);
                    heap.Push(ot.ID);
                }
                //get the next node from open list
                while (heap.Count > 0)
                {
                    var id = heap.Pop();
                    if (nodeList.TryGetValue(id, out min))
                    {
                        //if the next node is closed, skip it.
                        if (min.Closed)
                            continue;
                        min.Closed = true;
                        current = min.Value;
                        goto Loop;
                    }
                    goto NotFound;
                }
                goto NotFound;
            }
            End:
            return min.Path;
            NotFound:
            return ImmutableList<TEdge>.Empty;
        }
        
        internal struct AStarNode<TNode, TEdge, TGraph, THeap, TGFunc> :
            IEquatable<AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>>
            where TNode : IEquatable<TNode>, IIDHolder
            where TEdge : IEdge<TNode>
            where TGraph : IReadOnlyGraph<TNode, TEdge>
            where THeap : IHeap<int>
            where TGFunc : ICostFunc<TEdge>
        {
            public AStarNode(TNode value)
            {
                Value = value;
                CurrentG = float.PositiveInfinity;
                Priority = float.PositiveInfinity;
                Closed = false;
                Path = ImmutableList<TEdge>.Empty;
            }
        
            public TNode Value { get; }
            public float CurrentG { get; set; }
            public float Priority { get; set; }
            public ImmutableList<TEdge> Path { get; set; }
            public bool Closed { get; set; }
            public int ID => Value.ID;
            public bool Equals(AStarNode<TNode, TEdge, TGraph, THeap, TGFunc> other)
                => ID == other.ID;

            public override bool Equals(object obj) 
                => obj is AStarNode<TNode, TEdge, TGraph, THeap, TGFunc> other && Equals(other);

            public override int GetHashCode() => ID;
        }
    }
}
