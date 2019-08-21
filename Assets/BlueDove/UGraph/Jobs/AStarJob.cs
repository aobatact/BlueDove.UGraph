using System;
using BlueDove.UGraph.Algorithm;
using Unity.Jobs;
using Unity.Collections;

namespace BlueDove.UGraph.Jobs
{
    public struct NativeRefDictionary<TKey,TValue>
    {
        public ref TValue GetOrAddValueRef(TKey key) => throw new NotImplementedException();
        public bool TryGetValue(TKey key, out TValue value) => throw new NotImplementedException();
    }
    
    
    public struct AStarJob<TNode, TEndNode, TEdge, TGraph, THeap, TGFunc, THFunc> : IJob
        where TNode : unmanaged, IEquatable<TNode>, IIDHolder
        where TEdge : unmanaged, IEdge<TNode>
        where TGraph : unmanaged, IGraph<TNode, TEdge>
        where THeap : struct, IPriorityQueue<int>
        where TGFunc : unmanaged, ICostFunc<TEdge>
        where THFunc : unmanaged, ICostFunc<TNode>
        where TEndNode : unmanaged, IEquatable<TNode>
    {
        private TGraph graph;
        private TGFunc costFunc;
        private THFunc heuristicFunc;
        private THeap heap;
        private TNode start;
        private TEndNode end;
        private NativeRefDictionary<int, AStarNode> nodeList;
        
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
                    //aNode.Path = min.Path.Add(edge);
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
            //return min.Path;
            NotFound: ;
            
            openList.Dispose();
        }
        
        public struct AStarNode
        {
            public TNode Value { get; }
            public int ID => Value.ID;
            public float CurrentG { get; set; }
            public float Priority { get; set; }
            public bool Closed { get; set; }
            //public ImmutableList<TEdge> Path { get; set; }

            public AStarNode(TNode current) : this() { Value = current; }
        }
    }
}
