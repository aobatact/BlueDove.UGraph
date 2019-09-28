using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace BlueDove.UCollections.Native
{
    [NativeContainer]
    public unsafe struct NativeListList<T>:IDisposable where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
        private AtomicSafetyHandle[] m_InnerSafeties;
        [NativeSetClassTypeToNullOnSchedule]
        DisposeSentinel m_DisposeSentinel;
        private DisposeSentinel[] m_InnerDisposeSentinels;
#endif
        [NativeDisableUnsafePtrRestriction]
        internal UnsafePtrList* m_ListData;
        internal Allocator m_DeprecatedAllocator;

        public NativeListList(int initialCapacity, Allocator allocator)
            :this(initialCapacity,allocator,2){}
        
        private NativeListList(int initialCapacity, Allocator allocator, int disposeSentinelStackDepth)
        {
            var totalSize = UnsafeUtility.SizeOf<T>() * (long)initialCapacity;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent.
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity must be >= 0");

            CollectionHelper.CheckIsUnmanaged<T>();

            // Make sure we cannot allocate more than int.MaxValue (2,147,483,647 bytes)
            // because the underlying UnsafeUtility.Malloc is expecting a int.
            // TODO: change UnsafeUtility.Malloc to accept a UIntPtr length instead to match C++ API
            if (totalSize > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), $"Capacity * sizeof(T) cannot exceed {int.MaxValue} bytes");

            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, disposeSentinelStackDepth, allocator);
            m_InnerDisposeSentinels = new DisposeSentinel[initialCapacity];
            m_InnerSafeties = new AtomicSafetyHandle[initialCapacity];
#endif
            m_DeprecatedAllocator = allocator;
            m_ListData = (UnsafePtrList*) UnsafeList.Create(UnsafeUtility.SizeOf<T>(),
                UnsafeUtility.AlignOf<T>(), initialCapacity, allocator);
#if UNITY_2019_3_OR_NEWER && ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
        }

        public void AllocNext()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(m_Safety);
#endif
            var length = m_ListData->Length;
            if (m_InnerSafeties.Length == m_ListData->Length)
            {
                Array.Resize(ref m_InnerSafeties, length + 4);
                Array.Resize(ref m_InnerSafeties, length + 4);
            }

            DisposeSentinel.Create(out m_InnerSafeties[length], out m_InnerDisposeSentinels[length], 2,
                m_DeprecatedAllocator);
            m_ListData->Add(UnsafeList.Create(UnsafeUtility.SizeOf<T>(),
                UnsafeUtility.AlignOf<T>(), 4, m_DeprecatedAllocator));
        }

        public UnsafeList* GetUnsafeList(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if((uint)index >= m_ListData->Length)
                throw new ArgumentOutOfRangeException();
#endif
            return (UnsafeList*)m_ListData->Ptr[index];
        }

        public NativeList<T> GetList(int index)
        {
            var shim = new NativeListShim(m_InnerSafeties[index], null, GetUnsafeList(index), m_DeprecatedAllocator);
            return Unsafe.As<NativeListShim, NativeList<T>>(ref shim);
        }
        
        private struct NativeListShim
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            private AtomicSafetyHandle m_Safety;

            [NativeSetClassTypeToNullOnSchedule]
            private DisposeSentinel m_DisposeSentinel;
#endif
            [NativeDisableUnsafePtrRestriction] private UnsafeList* m_ListData;

            private Allocator m_DeprecatedAllocator;

            public NativeListShim(AtomicSafetyHandle mSafety, DisposeSentinel mDisposeSentinel, UnsafeList* mListData, Allocator mDeprecatedAllocator)
            {
                m_Safety = mSafety;
                m_DisposeSentinel = mDisposeSentinel;
                m_ListData = mListData;
                m_DeprecatedAllocator = mDeprecatedAllocator;
            }
        }
        
        public bool IsCreated => m_ListData != null;
        
        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            for (var i = 0; i < m_InnerDisposeSentinels.Length; i++)
            {
                if (m_InnerDisposeSentinels[i] != default)
                    DisposeSentinel.Dispose(ref m_InnerSafeties[i], ref m_InnerDisposeSentinels[i]);
            }

            m_InnerSafeties = null;
            m_InnerDisposeSentinels = null;
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            UnsafePtrList.Destroy(m_ListData);
        }
    }
}