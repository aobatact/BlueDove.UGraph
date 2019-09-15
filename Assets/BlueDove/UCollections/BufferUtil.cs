using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BlueDove.Collections.Heaps
{
    public static class BufferUtil
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Expand<T>(ref T[] buffer)
        {
            var nl = (uint)buffer.Length << 1;
            var nBuffer = new T[nl <= (uint) int.MaxValue ? nl :
                nl + (uint)(buffer.Length >> 1) <= (uint) int.MaxValue ? nl + (buffer.Length >> 1) : int.MaxValue];

#if NET_STANDARD_2_0
            Array.Copy(buffer,nBuffer,buffer.Length);
#else
            buffer.AsSpan().CopyTo(nBuffer);
#endif
            buffer = nBuffer;
        }

#if !NET_STANDARD_2_0
        [DoesNotReturn]
#endif
        public static void ThrowNoItem()
        {
            throw new InvalidOperationException("No Items in Heap");
        }
    }
}