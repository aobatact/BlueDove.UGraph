#define DEBUG_MARK

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using BlueDove.UCollections;
using Microsoft.Collections.Extensions;
using UnityEngine;

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
            where THeap : IHeap<KeyValuePair<float,int>>
            where TGFunc : ICostFunc<TEdge>
            where TEndNode : IEquatable<TNode>, ICostFunc<TNode>
        {
            var nodeList = new DictionarySlim<int, AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>>();
            TNode current;
            //If the start node fulfill the end condition, end the path finding.
            if (end.Equals(current = start))
                return ImmutableList<TEdge>.Empty;
            end.MarkColor(NodeType.End);
            var sNode = new AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>(current, true);
            ref var min = ref sNode; 
            nodeList.GetOrAddValueRef(min.ID) = min;
            current.MarkColor(NodeType.Start);
            while (true)
            {
                Loop:
                //update cost connected to current  
                foreach (var edge in graph.GetEdges(current))
                {
                    var ot = edge.Target;
                    if (end.Equals(ot))
                    {
                        current.MarkColor(NodeType.Close);
                        ot.MarkColor(NodeType.Current);
                        return min.Path.Add(edge);
                    }
                    ref var aNode = ref nodeList.GetOrAddValueRef(ot.ID);
                    if (!aNode.Initialized) aNode = new AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>(ot);
                    if (aNode.CurrentG <= min.CurrentG) continue;
                    var ng = min.CurrentG + costFunc.Calc(edge);
                    var nf = ng + end.Calc(ot);
                    if (aNode.Priority <= nf) continue;
                    aNode.Priority = nf;
                    aNode.CurrentG = ng;
                    aNode.Closed = false;
                    aNode.Path = min.Path.Add(edge);
                    aNode.Value.MarkColor(NodeType.Open);
                    heap.Push(new KeyValuePair<float, int>(nf, ot.ID));
                }
                current.MarkColor(NodeType.Close);
                //get the next node from open list
                while (heap.Count > 0)
                {
                    var id = heap.Pop();
                    min = ref nodeList.GetOrAddValueRef(id.Value);
                    if (min.Initialized)
                    {
                        //if the next node is closed, skip it.
                        if (min.Closed)
                            continue;
                        min.Closed = true;
                        current = min.Value;
                        current.MarkColor(NodeType.Current);
                        goto Loop;
                    }
                    goto NotFound;
                }
                goto NotFound;
            }
            NotFound:
            return ImmutableList<TEdge>.Empty;
        }

        private enum NodeType
        {
            None,
            Open,
            Current,
            Close,
            Start,
            End,
        }

        [Conditional("DEBUG_MARK")]
        private static void MarkColor<TNode>(this TNode node, NodeType type)
        {
            if (node is IMarkable markable)
            {
                switch (type)
                {
                    case NodeType.None:
                    default:
                        markable.Mark(Color.white);
                        break;
                    case NodeType.Open:
                        markable.Mark(Color.blue);
                        break;
                    case NodeType.Current:
                        markable.Mark(Color.green);
                        break;
                    case NodeType.Close:
                        markable.Mark(Color.red);
                        break;
                    case NodeType.Start:
                        markable.Mark(Color.magenta);
                        break;
                    case NodeType.End:
                        markable.Mark(Color.yellow);
                        break;
                }
            }
        }

        internal struct AStarNode<TNode, TEdge, TGraph, THeap, TGFunc> :
            IEquatable<AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>>
            where TNode : IEquatable<TNode>, IIDHolder
            where TEdge : IEdge<TNode>
            where TGraph : IReadOnlyGraph<TNode, TEdge>
            where THeap : IHeap<KeyValuePair<float, int>>
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

            public AStarNode(TNode node, bool startNode)
            {
                if (startNode)
                {
                    Value = node;
                    CurrentG = 0f;
                    Priority = 0f;
                    Closed = true;
                    Path = ImmutableList<TEdge>.Empty;
                }
                else
                    this = new AStarNode<TNode, TEdge, TGraph, THeap, TGFunc>(node);
            }
        
            public TNode Value { get; }
            public float CurrentG { get; set; }
            public float Priority { get; set; }
            public ImmutableList<TEdge> Path { get; set; }
            public bool Closed { get; set; }
            public bool Initialized => Path != null;
            public int ID => Value?.ID ?? 0;
            public bool Equals(AStarNode<TNode, TEdge, TGraph, THeap, TGFunc> other)
                => ID == other.ID;

            public override bool Equals(object obj) 
                => obj is AStarNode<TNode, TEdge, TGraph, THeap, TGFunc> other && Equals(other);

            public override int GetHashCode() => ID;
        }
    }
}
