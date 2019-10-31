using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace BlueDove.UGraph
{
    public static partial class GraphUtils
    {
        public static void GeneratePoints(NativeArray<float3> array, float xSize, float ySize, int xCount, int yCount)
        {
            if (array.Length != xCount * yCount)
            {
                throw new ArgumentException();
            }
            var v = float2.zero;
            var b = new float2(xSize, ySize);
            for (int i = 0; i < xCount; i++)
            {
                v.x = (xCount + 0.5f) * xSize;
                for (int j = 0; j < yCount; j++)
                {
                    v.y = (yCount + 0.5f) * ySize;
                    var n = noise.srdnoise(v).yz;
                    var m = math.normalize(n) * b;
                    array[j] = new float3(m, 0f);
                }
            }
        }

        public static NativeArray<T> GeneratePoints<T>(float xSize, float ySize, int xCount, int yCount,
            Func<float3, T> func, Allocator allocator) where T : struct
        {
            var nVec = new NativeArray<float3>(xCount * yCount, Allocator.Temp);
            var nar = new NativeArray<T>(xCount * yCount, allocator);
            GeneratePoints(nVec,xSize,ySize,xCount,yCount);
            for (int i = 0; i < nVec.Length; i++)
            {
                nar[i] = func(nVec[i]);
            }
            nVec.Dispose();
            return nar;
        }
    }
}
