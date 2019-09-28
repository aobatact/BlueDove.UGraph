using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if UNITY_2019_2_OR_NEWER
using Unity.Mathematics;
#endif

namespace BlueDove.Collections.Heaps
{
    public interface IUnsignedValueConverter<in T> : IComparer<T>
    {
        int GetIndex(T last, T value);
        int BufferSize();
    }

    public readonly struct UintValueConverter : IUnsignedValueConverter<uint>
    {
        public int GetIndex(uint last, uint value)
        {
            Debug.Assert(value >= last);
            if (last == value) return 0;
            return BitOperations.Log2(last ^ value) + 1;
        }

        public int BufferSize()
            => 33;

        public int Compare(uint x, uint y) 
            => x.CompareTo(y);
    }

    public readonly struct IntValueConverter : IUnsignedValueConverter<int>
    {
        public int GetIndex(int last, int value)
        {
            Debug.Assert(value >= last);
            if (last == value) return 0;
            return BitOperations.Log2((uint) (last ^ value)) + 1;
        }

        public int BufferSize()
            => 32;
        
        public int Compare(int x, int y) 
            => x.CompareTo(y);
    }

    public readonly struct UlongValueConverter : IUnsignedValueConverter<ulong>
    {
        public int GetIndex(ulong last, ulong value)
        {
            Debug.Assert(value >= last);
            if (last == value) return 0;
            return BitOperations.Log2(last ^ value) + 1;
        }

        public int BufferSize()
            => 65;

        public int Compare(ulong x, ulong y) => x.CompareTo(y);
    }

    public readonly struct LongValueConverter : IUnsignedValueConverter<long>
    {
        public int GetIndex(long last, long value)
        {
            Debug.Assert(value >= last);
            if (last == value) return 0;
            return BitOperations.Log2((ulong) (last ^ value)) + 1;
        }

        public int BufferSize()
            => 64;

        public int Compare(long x, long y) => x.CompareTo(y);
    }

    public readonly struct FloatValueConverter : IUnsignedValueConverter<float>
    {
        public int GetIndex(float last, float value)
        {
            Debug.Assert(last >= 0);
            Debug.Assert(value >= last);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (last == value) return 0;
            return BitOperations.Log2(Unsafe.As<float, uint>(ref last) ^ Unsafe.As<float, uint>(ref value)) + 1;
        }

        public int BufferSize()
            => 32;

        public int Compare(float x, float y) => x.CompareTo(y);
    }

    public readonly struct DoubleValueConverter : IUnsignedValueConverter<double>
    {
        public int GetIndex(double last, double value)
        {
            Debug.Assert(last >= 0);
            Debug.Assert(value >= last);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (last == value) return 0;
            return BitOperations.Log2(Unsafe.As<double, ulong>(ref last) ^ Unsafe.As<double, ulong>(ref value)) + 1;
        }

        public int BufferSize()
            => 64;

        public int Compare(double x, double y) => x.CompareTo(y);
    }

    public readonly struct KeyValueConverter<TKey, TValue, TConverter> : IUnsignedValueConverter<(TKey, TValue)>
        where TConverter : unmanaged, IUnsignedValueConverter<TKey> where TKey : IComparable<TKey>
    {
        public int GetIndex((TKey, TValue) last, (TKey, TValue) value)
            => default(TConverter).GetIndex(last.Item1, value.Item1);

        public int BufferSize()
            => default(TConverter).BufferSize();

        public int Compare((TKey, TValue) x, (TKey, TValue) y) 
            => default(TConverter).Compare(x.Item1, y.Item1);
    }

    public readonly struct FloatIntValueConverter : IUnsignedValueConverter<KeyValuePair<float,int>>
    {
        public int GetIndex(KeyValuePair<float, int> last, KeyValuePair<float, int> value) 
            => default(FloatValueConverter).GetIndex(last.Key, value.Key);

        public int BufferSize() => default(FloatValueConverter).BufferSize();
        public int Compare(KeyValuePair<float, int> x, KeyValuePair<float, int> y) 
            => x.Key.CompareTo(y.Key);
    }


#if NET_STANDARD_2_0
    internal static class BitOperations
    {
#if UNITY_2019_2_OR_NEWER
        public static int Log2(uint value) => 31 - math.lzcnt(value);
        public static int Log2(ulong value) => 63 - math.lzcnt(value);
#else
//copied form corefx
/*
The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
        public static int Log2(uint value) => Log2SoftwareFallback(value);
        public static int Log2(ulong value)
        {
            var hi = (uint)(value >> 32);
            if (hi == 0)
                return Log2((uint) value);
            return 32 + Log2(hi);
        }
        private static byte[] Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };
        private static int Log2SoftwareFallback(uint value)
        {
            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
            // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
            return Log2DeBruijn[(int)((value * 0x07C4ACDDu) >> 27)];
        }
#endif
    }
#endif
}