using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace BlueDove.UCollections.Native
{
    public static class NativeUcEx
    {
        public static unsafe void* Alloc<T>(T value, Allocator allocator)
            where T : struct
        {
            var mem = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator);
            ref var x = ref Unsafe.AsRef<T>(mem);
            x = value;
            return mem;
        }
    }
}