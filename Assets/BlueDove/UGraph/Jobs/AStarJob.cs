using System;
using BlueDove.UCollections;
using BlueDove.UGraph.Algorithm;
using Unity.Jobs;
using Unity.Collections;
// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace BlueDove.UGraph.Jobs
{
    public struct NativeRefDictionary<TKey,TValue>
    {
        public ref TValue GetOrAddValueRef(TKey key) => throw new NotImplementedException();
        public bool TryGetValue(TKey key, out TValue value) => throw new NotImplementedException();
    }
    
    
    public struct AStarJob<TNode, TEndNode, TEdge, TGraph, THeap, TGFunc, THFunc> : IJob
        where TNode : unmanaged, IEquatable<TNode>, IIDHolder
        where TEdge : unmanaged, IEdge<TNode>, IEquatable<TEdge>
        where TGraph : unmanaged, IReadOnlyNativeGraph<TNode, TEdge>
        where THeap : struct, IPriorityQueue<int>
        where TGFunc : unmanaged, ICostFunc<TEdge>
        where THFunc : unmanaged, ICostFunc<TNode>
        where TEndNode : unmanaged, IEquatable<TNode>
    {
        private readonly TGraph graph;
        private readonly TGFunc costFunc;
        private readonly THFunc heuristicFunc;
        private readonly THeap heap;
        private readonly NativeRefDictionary<int, AStarNode> nodeList;
        private readonly TNode start;
        private readonly TEndNode end;
        public readonly NativeList<TEdge> ReversePath;

        public AStarJob(TGraph graph, TGFunc costFunc, THFunc heuristicFunc, THeap heap, TNode start, TEndNode end,
            NativeList<TEdge> path, NativeRefDictionary<int, AStarNode> nodeList)
        {
            this.graph = graph;
            this.costFunc = costFunc;
            this.heuristicFunc = heuristicFunc;
            this.heap = heap;
            this.nodeList = nodeList;
            this.start = start;
            this.end = end;
            ReversePath = path;
        }

        public AStarJob(TGraph graph, TGFunc costFunc, THFunc heuristicFunc, THeap heap, TNode start, TEndNode end,
            Allocator allocator)
            : this(graph, costFunc, heuristicFunc, heap, start, end, new NativeList<TEdge>(allocator), default){}

        public void Execute()
        {
            var openList = new NativeHashMap<int, TNode>(32, Allocator.TempJob);
            TNode current;
            if (end.Equals(current = start))
                return;
            var min = new AStarNode(current);
            while (true)
            {
                Loop:
                //update cost connected to current  
                foreach (var edge in graph.GetEdges(current))
                {
                    var ot = edge.GetOther<TNode, TNode, TEdge>(current);
                    if (end.Equals(ot))
                    {
                        min = new AStarNode(ot) {RootPath = edge};
                        goto Found;
                    }
                    ref var aNode = ref nodeList.GetOrAddValueRef(ot.ID);
                    if (aNode.ID == 0)
                        aNode = new AStarNode(ot);
                    else if (aNode.CurrentG <= min.CurrentG) 
                        continue;
                    var ng = min.CurrentG + costFunc.Calc(edge);
                    var nf = ng + heuristicFunc.Calc(ot);
                    if (aNode.Priority <= nf) continue;
                    aNode.Priority = nf;
                    aNode.CurrentG = ng;
                    aNode.Closed = false;
                    //aNode.Path = min.Path.Add(edge);
                    aNode.RootPath = edge;
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
            Found:
            while (true)
            {
                var root = min.RootPath;
                if (root.Equals(default))
                    break;
                ReversePath.Add(root);
                var n = root.GetOther<TNode, TNode, TEdge>(min.Value);
                nodeList.TryGetValue(n.ID, out min);
            }
            
            
            //return min.Path;
            NotFound: ;
            
            openList.Dispose();
        }
        
        public struct AStarNode : IIDHolder
        {
            public TNode Value { get; }
            public int ID => Value.ID;
            public float CurrentG { get; set; }
            public float Priority { get; set; }
            public bool Closed { get; set; }
            //public ImmutableList<TEdge> Path { get; set; }
            public TEdge RootPath { get; set; }

            public AStarNode(TNode current) : this() { Value = current; }
        }
    }
}
