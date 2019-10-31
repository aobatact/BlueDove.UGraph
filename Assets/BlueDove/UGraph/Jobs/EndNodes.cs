using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace BlueDove.UGraph.Jobs
{
    public readonly struct EndNodes<TNode> : IEquatable<TNode>, IDisposable
        where TNode : struct, IIDHolder, IEquatable<TNode>
    {
        private readonly NativeArray<TNode> _nodes;
        private readonly bool _needDispose;

        public EndNodes(IEnumerable<TNode> nodes, Allocator allocator)
        {
            int c;
            switch (nodes)
            {
                case ICollection<TNode> col:
                    c = col.Count;
                    break;
                case IReadOnlyCollection<TNode> rcol:
                    c = rcol.Count;
                    break;
                default:
                    c = 4;
                    break;
            }
            var list = new NativeList<TNode>(c, allocator);
            foreach (var node in nodes)
            {
                list.Add(node);
            }
            _nodes = list.AsArray();
            _needDispose = true;
        }
        
        public EndNodes(NativeArray<TNode> nodes, bool needDispose, bool sorted)
        {
            _nodes = nodes;
            _needDispose = needDispose;
            if (!sorted)
            {
                _nodes.Sort<TNode, IDComparerS<TNode>>(default);
            }
        }

        //TODO Binary Search.
        public bool Equals(TNode other)
        {
            foreach (var node in _nodes)
            {
                if (node.Equals(other))
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            if (_needDispose)
                _nodes.Dispose();
        }
    }
}