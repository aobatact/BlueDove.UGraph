using System;
using System.Collections.Immutable;
using Microsoft.Collections.Extensions;

namespace BlueDove.UGraph.Algorithm
{
    /// <summary>
    /// Class to calculate A*
    /// </summary>
    /// <typeparam name="TNode">Node Type</typeparam>
    /// <typeparam name="TEdge">Edge Type</typeparam>
    /// <typeparam name="TGraph">Graph Type</typeparam>
    /// <typeparam name="THeap">Priority Queue to use inside</typeparam>
    /// <typeparam name="TGFunc">Function of calculating Cost in Edge</typeparam>
    /// <typeparam name="THFunc">Function of calculating heuristic cost for Node</typeparam>
    public class AStarAlgorithm<TNode, TEdge, TGraph, THeap, TGFunc, THFunc>
        where TNode : IEquatable<TNode>, IIDHolder
        where TEdge : IEdge<TNode>
        where TGraph : IGraph<TNode, TEdge>
        where THeap : IPriorityQueue<int>
        where TGFunc : ICostFunc<TEdge>
        where THFunc : ICostFunc<TNode>
    {
        private readonly TGraph graph;
        private readonly Func<THeap> heapFactory;
        private readonly TGFunc costFunc;
        private readonly THFunc heuristicFunc;

        /// <summary>
        /// Constructor of A* algorithm
        /// </summary>
        /// <param name="graph">Graph to search</param>
        /// <param name="heapFactory">Factory of creating heap</param>
        /// <param name="costFunc">Function of calculating Cost in Edge</param>
        /// <param name="heuristicFunc">Function of calculating heuristic cost for Node</param>
        public AStarAlgorithm(TGraph graph, Func<THeap> heapFactory, TGFunc costFunc, THFunc heuristicFunc)
        {
            this.graph = graph;
            this.heapFactory = heapFactory;
            this.costFunc = costFunc;
            this.heuristicFunc = heuristicFunc;
        }

        /// <summary>
        /// Compute the A* algorithm
        /// </summary>
        /// <param name="start">Start Node.</param>
        /// <param name="end">Set of EndNode. This could be single node of some end condition.</param>
        /// <typeparam name="TEndNode">Type to confirm the current node fulfill the end condition.</typeparam>
        /// <returns>Path to the start node to end node.</returns>
        public ImmutableList<TEdge> Compute<TEndNode>(TNode start, TEndNode end)
            where TEndNode : IEquatable<TNode>
        {
            DictionarySlim<int, AStarNode> nodeList= new DictionarySlim<int, AStarNode>();
            THeap heap = heapFactory();
            TNode current;
            //If the start node fulfill the end condition, end the path finding.
            if (end.Equals(current = start))
                return ImmutableList<TEdge>.Empty;
            var min = new AStarNode(current);
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
                    if (aNode.ID == 0) aNode = new AStarNode(ot);
                    var ng = min.CurrentG + costFunc.Calc(edge);
                    var nf = ng + heuristicFunc.Calc(ot);
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
                        if (end.Equals(current = min.Value))
                            goto End;
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
        
        internal struct AStarNode : IEquatable<AStarNode>, IEquatable<TNode>
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
            public int ID => Value.ID;
            public float CurrentG { get; set; }
            public float Priority { get; set; }
            public bool Closed { get; set; }
            public ImmutableList<TEdge> Path { get; set; }
            public bool Equals(AStarNode other)
            {
                return ID == other.ID;
            }

            public bool Equals(TNode other) => Value.Equals(other);

            public override bool Equals(object obj)
            {
                return obj is AStarNode other && Equals(other) ||
                       obj is TNode t && Value.Equals(t);
            }

            public override int GetHashCode() => ID;
        }
    }
}
