using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace BlueDove.UCollections.Native
{
    [NativeContainer]
    public unsafe struct NativeBinaryHeap<T> : IHeap<T>, IDisposable where T : unmanaged, IComparable<T>
    {
        private UnsafeList* _list;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule] private DisposeSentinel m_DisposeSentinel;
#endif

        public NativeBinaryHeap(Allocator allocator)
            : this(2, allocator)
        {
        }

        public NativeBinaryHeap(int initialCapacity, Allocator allocator)
        {
            _list = UnsafeList.Create(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(),
                initialCapacity < 2 ? 2 : math.ceilpow2(initialCapacity), allocator);
            _list->Length = 1;
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T GetHeadRef() => ref Unsafe.AsRef<T>(_list->Ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetByIndex(int index = 1) => Unsafe.Add(ref Unsafe.AsRef<T>(_list->Ptr), index);

        private void CascadeUp(int oldIndex, T value)
        {
            ref var listPtr = ref GetHeadRef();
            ref var cValue = ref Unsafe.Add(ref listPtr, oldIndex);
            var next = oldIndex;
            while (true)
            {
                if (next == 1) break;
                ref var nValue = ref Unsafe.Add(ref listPtr, next >>= 1);
                if (nValue.CompareTo(value) > 0)
                {
                    cValue = nValue;
                    cValue = ref nValue;
                    continue;
                }

                break;
            }

            cValue = value;
        }

        private void CascadeDown(int oldIndex, T value)
        {
            ref var listPtr = ref GetHeadRef();
            ref var cValue = ref Unsafe.Add(ref listPtr, oldIndex);
            ref var n = ref cValue;
            var length = _list->Length - 1;
            var index = oldIndex;
            while (true)
            {
                ref var lv = ref Unsafe.Add(ref listPtr, index <<= 1);
                if (index >= length)
                {
                    if (index == length)
                    {
                        cValue = lv;
                        cValue = ref lv;
                    }

                    break;
                }

                ref var rv = ref Unsafe.Add(ref lv, 1);
                if (lv.CompareTo(rv) > 0)
                {
                    index++;
                    n = ref rv;
                }
                else
                    n = ref lv;

                if (n.CompareTo(value) >= 0) break;
                cValue = n;
                cValue = ref n;
            }

            cValue = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            if (_list->Length == _list->Capacity) _list->SetCapacity<T>(_list->Capacity << 1);
            CascadeUp(_list->Length++, value);
        }

        public T Peek()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            if (_list->Length < 2) Thrower();
            return GetByIndex();
        }

        public T Pop()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            if (_list->Length < 2) Thrower();
            var top = GetByIndex();
            CascadeDown(1, GetByIndex(_list->Length - 1));
            _list->Length--;
            return top;
        }

        public bool TryPeek(out T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            if (_list->Length < 2)
            {
                value = default;
                return false;
            }

            value = GetByIndex();
            return true;
        }

        public bool TryPop(out T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            if (_list->Length < 2)
            {
                value = default;
                return false;
            }

            value = GetByIndex();
            CascadeDown(1, GetByIndex(_list->Length - 1));
            _list->Length--;
            return true;
        }

        public int Count
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return _list->Length - 1;
            }
        }

        public void Clear()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(m_Safety);
#endif
            _list->Length = 1;
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
            UnsafeList.Destroy(_list);
            _list = null;
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
            var jobHandle = new DisposeJob {Container = this}.Schedule(inputDeps);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_Safety);
#endif
            return jobHandle;
        }

        //[BurstCompile]
        private struct DisposeJob : IJob
        {
            public NativeBinaryHeap<T> Container;

            public void Execute()
            {
                Container.Deallocate();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Thrower() => throw new InvalidOperationException();
    }
}