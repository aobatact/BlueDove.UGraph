using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace BlueDove.UCollections
{
    public class NativeArrayEnumerableWrapper<T> : IEnumerable<T> where T : struct
    {
        private NativeArray<T> _array;
        private readonly bool _needDispose;

        public NativeArrayEnumerableWrapper(NativeArray<T> array, bool needDispose)
        {
            _array = array;
            _needDispose = needDispose;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this, _array.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<T>
        {
            private readonly NativeArrayEnumerableWrapper<T> _wrapper;
            private NativeArray<T>.Enumerator _enumerator;

            public Enumerator(NativeArrayEnumerableWrapper<T> wrapper, NativeArray<T>.Enumerator enumerator)
            {
                _wrapper = wrapper;
                _enumerator = enumerator;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() 
                => _enumerator.MoveNext();


            public T Current => _enumerator.Current;

            public void Dispose()
            {
                if (_wrapper._needDispose)
                {
                    _wrapper._array.Dispose();
                }
            }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }
            
        }
    }
}