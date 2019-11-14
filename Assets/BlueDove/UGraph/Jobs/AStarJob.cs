using System;
using BlueDove.UCollections;
using BlueDove.UGraph.Algorithm;
using Unity.Jobs;
using Unity.Collections;

namespace BlueDove.UGraph.Jobs
{
    /// <summary>
    /// AStar PathFinding Job
    /// </summary>
    /// <typeparam name="TNode">Node Type in Graph</typeparam>
    /// <typeparam name="TEndNode">End node condition of <see cref="TNode"/>>, might be <see cref="TNode"/>></typeparam>
    /// <typeparam name="TEdge">Edge Type in Graph</typeparam>
    /// <typeparam name="TGraph">Graph Type to Search</typeparam>
    /// <typeparam name="THeap">Priority Queue Used inside</typeparam>
    /// <typeparam name="TGFunc">Cost function to calculate the edge cost</typeparam>
    public readonly struct AStarJob<TNode, TEndNode, TEdge, TGraph, THeap, TGFunc> : IJob
        where TNode : struct, IEquatable<TNode>, IIDHolder
        where TEdge : struct, IEdge<TNode>, IEquatable<TEdge>
        where TGraph : struct, IReadOnlyNativeGraph<TNode, TEdge>
        where THeap : struct, IHeap<int>
        where TGFunc : struct, ICostFunc<TEdge>
        where TEndNode : struct, IEquatable<TNode>, ICostFunc<TNode>
    {
        private readonly TGraph _graph;
        private readonly TGFunc _costFunc;
        private readonly THeap _heap;
        private readonly TNode _start;
        private readonly TEndNode _end;
        private readonly NativeList<TEdge> _reversePath;
        private readonly Allocator _allocator;

        /// <summary>
        /// Creating a job
        /// </summary>
        /// <param name="graph">Graph to Search</param>
        /// <param name="costFunc">Cost Function to evaluate the edge cost</param>
        /// <param name="heap">PriorityQueue used inside</param>
        /// <param name="start">Node to Start</param>
        /// <param name="end">Node Condition to finish</param>
        /// <param name="reversePath">Container for Reversed Result</param>
        /// <param name="allocator">Allocator for inner Collection.</param>
        public AStarJob(TGraph graph, TGFunc costFunc, THeap heap, TNode start, TEndNode end,
            NativeList<TEdge> reversePath, Allocator allocator = Allocator.TempJob)
        {
            _graph = graph;
            _costFunc = costFunc;
            _heap = heap;
            _start = start;
            _end = end;
            _reversePath = reversePath;
            _allocator = allocator;
        }

        /// <summary>
        /// Run the AStar
        /// </summary>
        public void Execute()
        {
            var openList = new NativeHashMap<int, AStarNode>(32, _allocator);
            TNode current;
            if (_end.Equals(current = _start))
                return;
            var min = new AStarNode(current, true);
            openList.TryAdd(current.ID, min);
            while (true)
            {
                Loop:
                //update node cost connected to current node  
                var edges = _graph.GetEdges(current, _allocator);
                for (var i = 0; i < edges.Length; i++)
                {
                    var edge = edges[i];
                    var otherSide = edge.Target;
                    if (_end.Equals(otherSide))
                    {
                        min = new AStarNode(otherSide) {RootPath = edge};
                        goto Found;
                    }

                    if (openList.TryGetValue(otherSide.ID, out var aNode))
                    {
                        if (aNode.CurrentG <= min.CurrentG) continue;
                    }
                    else
                    {
                        aNode.Priority = float.MaxValue;
                    }

                    var ng = min.CurrentG + _costFunc.Calc(edge);
                    var nf = ng + _end.Calc(otherSide);
                    // if the cost might be higher to use this route, drop it.
                    if (aNode.Priority <= nf) continue;
                    //remove if GetOrAddValueRef
                    openList[otherSide.ID] = new AStarNode(otherSide, ng, nf, false, edge);
                    _heap.Push(otherSide.ID);
                }

                if (_graph.RequireFreeArray)
                    edges.Dispose();
                //get the next node from open list
                while (true)
                {
                    if (_heap.Count > 0 && openList.TryGetValue(_heap.Pop(), out min))
                    {
                        //if the next node is closed, skip it.
                        if (min.Closed)
                            continue;
                        min.Closed = true;
                        openList[(current = min.Value).ID] = min;
                        goto Loop;
                    }

                    goto NotFound;
                }
            }

            Found:
            while (true)
            {
                var root = min.RootPath;
                if (root.Equals(default))
                    break;
                _reversePath.Add(root);
                var n = root.Target;
                openList.TryGetValue(n.ID, out min);
            }

            //return min.Path;
            NotFound: ;
            openList.Dispose();
        }

        private struct AStarNode : IIDHolder
        {
            /// <summary>
            /// Node to represent
            /// </summary>
            public TNode Value { get; }

            public int ID => Value.ID;

            /// <summary>
            /// Current Cost from the Start
            /// </summary>
            public float CurrentG { get; }

            /// <summary>
            /// Current Priority Value (Lower is Prior)
            /// </summary>
            public float Priority { get; set; }

            /// <summary>
            /// Check whether it can be the next node to search neighbor
            /// </summary>
            public bool Closed { get; set; }

            /// <summary>
            /// Edge toward the start node
            /// </summary>
            public TEdge RootPath { get; set; }

            public AStarNode(TNode current) : this()
            {
                Value = current;
            }

            public AStarNode(TNode current, bool closed) : this()
            {
                Value = current;
                Closed = closed;
            }

            public AStarNode(TNode value, float currentG, float priority, bool closed, TEdge rootPath)
            {
                Value = value;
                CurrentG = currentG;
                Priority = priority;
                Closed = closed;
                RootPath = rootPath;
            }
        }
    }
}