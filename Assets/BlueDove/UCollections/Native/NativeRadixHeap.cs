//#define ENABLE_UNITY_COLLECTIONS_CHECKS

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace BlueDove.UCollections.Native
{
    [NativeContainer]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeRadixHeap<T, TConverter> : IHeap<T>, IDisposable
        where T : struct
        where TConverter : struct, IUnsignedValueConverter<T>
    {
        private static int BufferSize => default(TConverter).BufferSize();

        private readonly UnsafeList* _buffer;
        public int Count { get; private set; }
        private readonly Allocator m_AllocatorLabel;
        private T Last { get; set; }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule]
        private DisposeSentinel m_DisposeSentinel;
#endif
        public NativeRadixHeap(Allocator allocator)
        {
            _buffer = (UnsafeList*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * BufferSize,
                UnsafeUtility.AlignOf<T>(), m_AllocatorLabel = allocator);
            Count = 0;
            Last = default;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
#endif
            for (var i = 0; i < BufferSize; i++)
            {
                _buffer[i] = new UnsafeList(allocator);
            }
        }

        public NativeRadixHeap(int initialCount, Allocator allocator)
        {
            _buffer = (UnsafeList*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * BufferSize,
                UnsafeUtility.AlignOf<T>(), m_AllocatorLabel = allocator);
            Count = 0;
            Last = default;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
#endif
            for (var i = 0; i < BufferSize; i++)
            {
                _buffer[i] = new UnsafeList(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(),
                    initialCount, allocator);
            }
        }

        public void Push(T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(m_Safety);
#endif
            _buffer[default(TConverter).GetIndex(Last, value)].Add(value);
            Count++;
        }

        public T Peek()
        {
            if (Count == 0) BufferUtil.ThrowNoItem();
            Pull();
            return Last;
        }

        public T Pop()
        {
            if (Count == 0) BufferUtil.ThrowNoItem();
            Pull();
            _buffer[0].Length--;
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
            _buffer[0].Length--;

            value = Last;
            Count--;
            return true;
        }

        private void Pull()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(m_Safety);
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            if (Count == 0)
                BufferUtil.ThrowNoItem();
#endif
            var zero = _buffer;
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
                    if (i + 1 >= BufferSize)
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

        public void Clear()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(m_Safety);
#endif
            for (var i = 0; i < BufferSize; i++) _buffer[i].Clear();
            Count = 0;
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            Deallocate();
        }

        private void Deallocate()
        {
            if (m_AllocatorLabel != Allocator.Invalid)
            {
                for (var i = 0; i < BufferSize; i++)
                {
                    _buffer[i].Dispose();
                }
                UnsafeUtility.Free(_buffer, m_AllocatorLabel);
                this = default;
            }
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // [DeallocateOnJobCompletion] is not supported, but we want the deallocation
            // to happen in a thread. DisposeSentinel needs to be cleared on main thread.
            // AtomicSafetyHandle can be destroyed after the job was scheduled (Job scheduling
            // will check that no jobs are writing to the container).
            DisposeSentinel.Clear(ref m_DisposeSentinel);
#endif
            var jobHandle = new DisposeJob(this).Schedule(inputDeps);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_Safety);
#endif
            return jobHandle;
        }

        //[BurstCompile]
        private struct DisposeJob : IJob
        {
            public NativeRadixHeap<T, TConverter> Container;

            public DisposeJob(NativeRadixHeap<T, TConverter> container) => Container = container;

            public void Execute() => Container.Deallocate();
        }
    }
}
