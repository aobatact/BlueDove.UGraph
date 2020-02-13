using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace BlueDove.UCollections.Native
{
    [NativeContainer]
    public struct NativeFixedSizedRadixHeap<T, TConverter>
        where T : struct
        where TConverter : struct, IUnsignedValueConverter<T>
    {
        private NativeArray<T> values;
        private int Count;
        private T Last { get; set; }
        private T Max { get; set; }

    }

    
    [NativeContainer]
    public struct NativeRadixHeap<T, TConverter> : IHeap<T>, IDisposable
        where T : struct
        where TConverter : struct, IUnsignedValueConverter<T>
    {
        private NativeArray<UnsafeList> _nativeArray;
        public int Count { get; private set; }
        private T Last { get; set; }

        public NativeRadixHeap(Allocator allocator)
        {
            _nativeArray = new NativeArray<UnsafeList>(default(TConverter).BufferSize(), allocator);
            Count = 0;
            Last = default;
            InitAlloc(allocator);
        }

        public NativeRadixHeap(int initialCount, Allocator allocator)
        {
            _nativeArray = new NativeArray<UnsafeList>(default(TConverter).BufferSize(), allocator);
            Count = 0;
            Last = default;
            InitAlloc(initialCount, allocator);
        }

        private void InitAlloc(Allocator allocator)
        {
            for (var i = 0; i < _nativeArray.Length; i++)
            {
                _nativeArray[i] = new UnsafeList(allocator);
            }
        }

        private void InitAlloc(int initialCount, Allocator allocator)
        {
            for (var i = 0; i < _nativeArray.Length; i++)
            {
                _nativeArray[i] = new UnsafeList(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(),
                    initialCount, allocator);
            }
        }

        public void Push(T value)
        {
            unsafe
            {
                Unsafe.Add(ref Unsafe.AsRef<UnsafeList>(_nativeArray.GetUnsafePtr()),
                    default(TConverter).GetIndex(Last, value)).Add(value);
            }
            Count++;
        }

        public T Peek()
        {
            if (Count == 0) Thrower();
            Pull();
            return Last;
        }

        public T Pop()
        {
            if (Count == 0) Thrower();
            Pull();
            unsafe
            {
                //UnsafeUtilityEx.ArrayElementAsRef<UnsafeList>(_nativeArray.GetUnsafePtr(), 0).Length--;
                ((UnsafeList*) _nativeArray.GetUnsafePtr())->Length--;
            }
            Count--;
            return Last;
        }

        public bool TryPeek(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            Pull();
            value = Last;
            return true;
        }

        public bool TryPop(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            Pull();
            unsafe
            {
                ((UnsafeList*) _nativeArray.GetUnsafePtr())->Length--;
            }

            value = Last;
            Count--;
            return true;
        }

        private void Pull()
        {
            unsafe
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (Count == 0)
                    BufferUtil.ThrowNoItem();
#endif
                var zero = (UnsafeList*) _nativeArray.GetUnsafePtr();

                if (zero->Length == 0)
                {
                    var ptr = zero;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    var i = 0;
#endif
                    while ((++ptr)->Length == 0)
                    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                        ++i;
                        if (i + 1 >= _nativeArray.Length)
                            BufferUtil.ThrowNoItem();
#endif
                    }

                    ref var ptrPtr = ref Unsafe.AsRef<T>(ptr->Ptr);
                    var min = ptrPtr;
                    var length = ptr->Length;
                    for (var j = 1; j < length; j++)
                    {
                        var current = Unsafe.Add(ref ptrPtr, j);
                        if (default(TConverter).Compare(min, current) > 0)
                            min = current;
                    }

                    for (var j = 0; j < length; j++)
                    {
                        var value = Unsafe.Add(ref ptrPtr, j);
                        var elementOffset = default(TConverter).GetIndex(min, value);
                        zero[elementOffset].Add(value);
                    }

                    ptr->Clear();
                }

                Last = Unsafe.Add(ref Unsafe.AsRef<T>(zero->Ptr), zero->Length - 1);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _nativeArray.Length; i++) _nativeArray[i].Clear();
            Count = 0;
        }

        private static void Thrower() => throw new InvalidOperationException();

        public void Dispose()
        {
            Deallocate();
        }

        private void Deallocate()
        {
            for (var i = 0; i < _nativeArray.Length; i++)
            {
                _nativeArray[i].Dispose();
            }

            _nativeArray.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            var jobHandle = new DisposeJob {Container = this}.Schedule(inputDeps);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(_nativeArray));
#endif
            return jobHandle;
        }

        //[BurstCompile]
        private struct DisposeJob : IJob
        {
            public NativeRadixHeap<T, TConverter> Container;

            public void Execute()
            {
                Container.Deallocate();
            }
        }
    }
}