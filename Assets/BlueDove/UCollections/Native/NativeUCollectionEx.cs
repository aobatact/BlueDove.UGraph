using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace BlueDove.UCollections.Native
{
    public static class NativeUCollectionEx
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

    public unsafe struct Box<T> : IDisposable where T : struct
    {
        private readonly void* value;
        private readonly Allocator _allocator;

        public Box(T value, Allocator allocator)
            => this.value = NativeUCollectionEx.Alloc(value, _allocator = allocator);

        public ref T Value => ref Unsafe.AsRef<T>(value);

        public void Dispose()
        {
            UnsafeUtility.Free(value, _allocator);
            this = default;
        }
    }
}