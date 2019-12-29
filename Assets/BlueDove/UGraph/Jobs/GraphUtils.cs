using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace BlueDove.UGraph
{
    public static partial class GraphUtils
    {
        public interface IPointFromFloat2<out T>
        {
            T Create(float2 v);
        }

        public readonly struct Convert2To3 : IPointFromFloat2<float3>
        {
            public float3 Create(float2 v) => new float3(v, 0f);
        }

        public struct GenerateFlatPointsJob<T, TPointConverter> : IJob
            where T : unmanaged where TPointConverter : IPointFromFloat2<T>
        {
            private NativeArray<T> array;
            private readonly float2 size;
            private readonly int2 count;
            private Random random;
            TPointConverter _pointConverter;

            public GenerateFlatPointsJob(NativeArray<T> array, float2 size, int2 count, Random random,
                TPointConverter converter)
            {
                this.array = array;
                this.size = size;
                this.count = count;
                this.random = random;
                _pointConverter = converter;
                if (array.Length != count.x * count.y)
                {
                    throw new ArgumentException();
                }
            }

            public void Execute()
            {
                var v = float2.zero;
                for (int i = 0, k = -1; i < count.x; i++)
                {
                    for (int j = 0; j < count.y; j++)
                    {
                        var xy = v + random.NextFloat2(size);
                        //var xy = noise.cellular2x2(v);
                        array[++k] = _pointConverter.Create(xy);
                        v.y += size.y;
                    }
                    v.y = 0f;
                    v.x += size.x;
                }
            }
        }

        public static JobHandle JobGeneratePoints(NativeArray<float3> array, float2 size, int2 count, Random random,
            JobHandle inputDeps = default)
            => new GenerateFlatPointsJob<float3, Convert2To3>(array, size, count, random, default)
                .Schedule(inputDeps);

        public static NativeArray<T> GeneratePoints<T>(float2 size, int2 count, Func<float3, T> func,
            Allocator allocator, Random random) where T : struct
        {
            var length = count.x * count.y;
            var nVec = new NativeArray<float3>(length, Allocator.TempJob);
            var nar = new NativeArray<T>(length, allocator);
            JobGeneratePoints(nVec, size, count, random).Complete();
            for (int i = 0; i < nVec.Length; i++)
            {
                nar[i] = func(nVec[i]);
            }
            nVec.Dispose();
            return nar;
        }

        public static T[] GeneratePoints<T>(float2 size, int2 count, Func<float3, T> func, uint seed = 12)
            => GeneratePoints(size, count, func, new Random(seed));

        public static T[] GeneratePoints<T>(float2 size, int2 count, Func<float3, T> func, Random random)
        {
            var length = count.x * count.y;
            var nVec = new NativeArray<float3>(length, Allocator.TempJob);
            var nar = new T[length];
            JobGeneratePoints(nVec, size, count, random).Complete();
            for (int i = 0; i < nVec.Length; i++)
            {
                nar[i] = func(nVec[i]);
            }
            nVec.Dispose();
            return nar;
        }
    }
}
