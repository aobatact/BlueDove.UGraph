using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Collections.Extensions;

namespace BlueDove.UGraph
{
    public readonly struct BidirectionalGraph<TNode, TEdge> : IGraph<TNode, DirectionalEdge<TNode, TEdge>>
        where TNode : IEquatable<TNode>, IIDHolder
        where TEdge : IEdge<TNode>, IEquatable<TEdge>
    {
        private readonly DictionarySlim<TNode, List<DirectionalEdge<TNode, TEdge>>> _dictionary;

        public BidirectionalGraph(int initCapacity)
        {
            _dictionary = new DictionarySlim<TNode, List<DirectionalEdge<TNode, TEdge>>>(initCapacity);
        }
        
        public BidirectionalGraph(DictionarySlim<TNode, List<DirectionalEdge<TNode, TEdge>>> dictionary)
        {
            _dictionary = dictionary;
        }

        public bool Contains(TNode node)
            => _dictionary.ContainsKey(node);

        public bool Contains(TEdge edge)
            => _dictionary.TryGetValue(edge.Source, out var edges) && edges.Any(x => x.Edge.Equals(edge));

        public bool Contains(DirectionalEdge<TNode, TEdge> edge)
            => _dictionary.TryGetValue(edge.Source, out var nEdge) && nEdge.Contains(edge);

        public DirectionalEdge<TNode, TEdge> GetEdge(TNode source, TNode target, out bool found)
        {
            if (_dictionary.TryGetValue(source, out var list))
            {
                foreach (var edge in list)
                {
                    if (!edge.Target.Equals(target)) continue;
                    found = true;
                    return edge;
                }
            }

            found = false;
            return default;
        }

        public IEnumerable<DirectionalEdge<TNode, TEdge>> GetEdges()
            => _dictionary.SelectMany(pair => pair.Value.Where(edge => edge.Target.ID > edge.Source.ID));

        public IEnumerable<DirectionalEdge<TNode, TEdge>> GetEdges(TNode node) =>
            _dictionary.TryGetValue(node, out var list)
                ? list
                : Enumerable.Empty<DirectionalEdge<TNode, TEdge>>();

        public IEnumerable<TNode> GetNodes()
            => _dictionary.Select(x => x.Key);

        public bool AcceptDuplicateEdges => false;

        public bool AddEdge(TEdge edge)
            => AddEdge(new DirectionalEdge<TNode, TEdge>(edge, true));

        public bool AddNode(TNode node)
        {
            ref var list = ref _dictionary.GetOrAddValueRef(node);
            if (list == null)
            {
                list = new List<DirectionalEdge<TNode, TEdge>>();
                return true;
            }
            return false;
        }

        public bool AddEdge(DirectionalEdge<TNode, TEdge> edge)
        {
            ref var listS = ref _dictionary.GetOrAddValueRef(edge.Source);
            if (listS == null)
            {
                listS = new List<DirectionalEdge<TNode, TEdge>>();
            }

            listS.Add(edge);
            ref var listT = ref _dictionary.GetOrAddValueRef(edge.Target);
            if (listT == null)
            {
                listT = new List<DirectionalEdge<TNode, TEdge>>();
            }

            listT.Add(edge.Reverse());
            return true;
        }

        public bool RemoveNode(TNode node) => _dictionary.Remove(node);

        public bool RemoveEdge(DirectionalEdge<TNode, TEdge> edge)
        {
            if (_dictionary.TryGetValue(edge.Source,out var list))
            {
                list.Remove(edge);
                if (_dictionary.TryGetValue(edge.Target, out var tList))
                {
                    tList.Remove(edge.Reverse());
                }

                return true;
            }

            return false;
        }

        public void Clear() => _dictionary.Clear();
    }
}