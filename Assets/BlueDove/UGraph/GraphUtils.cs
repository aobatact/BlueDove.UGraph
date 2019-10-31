using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Collections.Extensions;
using Unity.Collections;
using UnityEngine;

namespace BlueDove.UGraph
{
    public static partial class GraphUtils
    {
        public static void CreateEdges<TNode, TEdge, TGraph>(TGraph graph, float minDistSq, float minAngle,
            Func<TNode, TNode, TEdge> func) 
            where TNode : IEquatable<TNode>, IIDHolder, IVector3Node
            where TEdge : IEdge<TNode>
            where TGraph : IGraph<TNode, TEdge>
        {
            var nodes = graph.GetNodes().ToArray();
            var addDict = new DictionarySlim<TNode, List<(TNode node,float dist)>>();
            for (var i = 0; i < nodes.Length; i++)
            {
                var nodeA = nodes[i];
                ref var list = ref addDict.GetOrAddValueRef(nodeA);
                if (list == null) list = new List<(TNode,float)>();
                for (var j = i + 1; j < nodes.Length; j++)
                {
                    var nodeB = nodes[j];
                    var vec = nodeB.Position - nodeA.Position;
                    var distSq = Vector3.SqrMagnitude(vec);
                    if (distSq > minDistSq)
                        continue;
                    var dist = Mathf.Sqrt(distSq);
                    list.Add((nodeB,dist));
                    ref var bList = ref addDict.GetOrAddValueRef(nodeB);
                    if (bList == null) bList = new List<(TNode,float)>();
                    bList.Add((nodeA, dist));
                }
            }

            var cos = 2 * (Mathf.Cos(minAngle) + 1f);
            var removeStack = new Stack<int>();
            for (int i = 0; i < nodes.Length; i++)
            {
                removeStack.Clear();
                var nodeA = nodes[i];
                if (!addDict.TryGetValue(nodeA, out var listA)) continue;
                listA.Sort((x, y) => x.dist.CompareTo(y.dist));
                for (var j = 0; j < listA.Count; j++)
                {
                    var (nodeB, ab) = listA[j];
                    addDict.TryGetValue(nodeB, out var listB);
                    for (var k = 0; k < j; k++) // test ab
                    {
                        if(removeStack.Contains(k))
                            continue;
                        var (nodeC, ac) = listA[k];
                        for (var l = 0; l < listB.Count; l++)
                        {
                            var (nodeCx, bc) = listB[l];
                            if (!nodeCx.Equals(nodeC)) continue;
                            if (ab < bc) // bc is longest
                            {
                                var x = ab + ac;
                                if (x * x < bc * bc + cos * ab * ac)
                                {
                                    //remove bc
                                    listB.RemoveAt(l);
                                    if (addDict.TryGetValue(nodeC, out var listC))
                                    {
                                        for (var m = 0; m < listC.Count; m++)
                                        {
                                            if (!listC[m].node.Equals(nodeB)) continue;
                                            listC.RemoveAt(m);
                                            break;
                                        }
                                    }
                                }
                            }
                            else // ab is longest
                            {
                                var x = ac + bc;
                                if (x * x < ab * ab + cos * ac * bc)
                                {
                                    //remove ab
                                    removeStack.Push(j);
                                    goto EndB;
                                }
                            }
                            break;
                        }
                    }
                    EndB: ;
                }

                while (removeStack.Count > 0)
                {
                    var rm = removeStack.Pop();
                    var (nodeN, _) = listA[rm];
                    listA.RemoveAt(rm);
                    if (!addDict.TryGetValue(nodeN, out var listN)) continue;
                    for (var j = 0; j < listN.Count; j++)
                    {
                        if (!listN[j].node.Equals(nodeA)) continue;
                        listN.RemoveAt(j);
                        break;
                    }
                }
            }

            foreach (var pair in addDict)
            {
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    var (node, _) = pair.Value[i];
                    if (node.ID > pair.Key.ID)
                    {
                        graph.AddEdge(func(pair.Key, node));
                    }
                }
            }
        }
    }
}