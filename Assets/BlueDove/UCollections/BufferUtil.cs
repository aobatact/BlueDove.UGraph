using System;
using System.Runtime.CompilerServices;

namespace BlueDove.UCollections
{
    public static class BufferUtil
    {

#if NET_STANDARD_2_1
        [DoesNotReturn]
#endif
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNoItem()
        {
            throw new InvalidOperationException("No Items in Heap");
        }
    }
}