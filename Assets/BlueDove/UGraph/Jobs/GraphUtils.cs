using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace BlueDove.UGraph
{
    public static partial class GraphUtils
    {
        public static void GeneratePoints(NativeArray<float3> array, float2 size, int2 count, uint seed = 12)
        {
            if (array.Length != count.x * count.y)
            {
                throw new ArgumentException();
            }
            var v = float2.zero;
            var ran = new Random(seed);
            for (int i = 0, k = -1; i < count.x; i++)
            {
                for (int j = 0; j < count.y; j++)
                {
                    var xy = v + ran.NextFloat2(size);
                    //var xy = noise.cellular2x2(v);
                    array[++k] = new float3(xy, 0f);
                    v.y += size.y;
                }
                v.y = 0f;
                v.x += size.x;
            }
        }

        public static NativeArray<T> GeneratePoints<T>(float2 size, int2 count, Func<float3, T> func, 
            Allocator allocator, uint seed = 12) where T : struct
        {
            var length = count.x * count.y;
            var nVec = new NativeArray<float3>(length, Allocator.Temp);
            var nar = new NativeArray<T>(length, allocator);
            GeneratePoints(nVec, size, count, seed);
            for (int i = 0; i < nVec.Length; i++)
            {
                nar[i] = func(nVec[i]);
            }
            nVec.Dispose();
            return nar;
        }
        
        public static T[] GeneratePoints<T>(float2 size, int2 count, Func<float3, T> func, uint seed = 12)
        {            
            var length = count.x * count.y;
            var nVec = new NativeArray<float3>(length, Allocator.Temp);
            var nar = new T[length];
            GeneratePoints(nVec, size, count, seed);
            for (int i = 0; i < nVec.Length; i++)
            {
                nar[i] = func(nVec[i]);
            }
            nVec.Dispose();
            return nar;
        }
    }
}
