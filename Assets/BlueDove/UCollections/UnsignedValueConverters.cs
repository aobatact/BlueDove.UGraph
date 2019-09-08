using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace BlueDove.Collections.Heaps
{
    public interface IUnsignedValueConverter<in T> where T : IComparable<T>
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
            => 33;
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
            => 65;
    }
    
    public readonly struct KeyValueConverter<TKey, TValue, TConverter> : IUnsignedValueConverter<(TKey, TValue)>
        where TConverter : unmanaged, IUnsignedValueConverter<TKey> where TKey : IComparable<TKey>
    {
        public int GetIndex((TKey, TValue) last, (TKey, TValue) value) 
            => default(TConverter).GetIndex(last.Item1, value.Item1);

        public int BufferSize() 
            => default(TConverter).BufferSize();
    }
    
    #if NET_STANDARD_2_0 && UNITY_2019_2_OR_NEWER
    public static class BitOperations
    {
        public static int Log2(uint value) => 31 - math.lzcnt(value);

        public static int Log2(ulong value) => 63 - math.lzcnt(value);
    }
    #endif
}