using System;
using System.Collections;
using BlueDove.Collections.Heaps;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace BlueDove.UCollections.Tests
{
    public class HeapTest
    {
        private static IEnumerable HeapHead<T>(IHeap<T> heap, Func<T> randomFactory, int count)
            where T : IComparable<T>
        {
            Assert.AreEqual(0, heap.Count,"This test should start with empty heap");
            var first = randomFactory();
            heap.Push(first);
            Assert.AreEqual(first, heap.Peek());
            var min = first;
            for (var i = 0; i < count; i++)
            {
                var next = randomFactory();
                if (next.CompareTo(min) < 0)
                {
                    min = next;
                }
                heap.Push(next);
                Assert.AreEqual(min, heap.Peek());
                yield return null;
            }
        }

        private static IEnumerable CheckSorted<T>(IHeap<T> heap, Func<T> randomFactory, int count)
            where T : IComparable<T>
        {
            Assert.AreEqual(0, heap.Count,"This test should start with empty heap");
            for (var i = 0; i < count; i++)
            {
                heap.Push(randomFactory());
                yield return null;
            }

            Assert.AreEqual(count, heap.Count);
            var x = heap.Pop();
            while (heap.Count != 0)
            {
                var n = heap.Pop();
                Assert.IsTrue(x.CompareTo(n) <= 0);
                x = n;
                yield return null;
            }
            Assert.IsFalse(heap.TryPeek(out _));
        }
        
        [UnityTest]
        public IEnumerator HeapHead_NativeBinary_Temp_Int()
        {
            using (var heap = new NativeBinaryHeap<int>(Allocator.Temp))
            {
                var random = new Random();
                foreach (var o in HeapHead(heap, () => random.Next(), 100))
                {
                    yield return o;
                }
            }
        }

        [UnityTest]
        public IEnumerator HeapHead_NativeRadix_Temp_Int()
        {
            using (var heap = new NativeRadixHeap<int, IntValueConverter>(Allocator.Temp))
            {
                var random = new Random();
                var prev = 0;
                foreach (var o in HeapHead(heap, 
                    () => prev == int.MaxValue ? int.MaxValue : prev = random.Next(prev, int.MaxValue), 10))
                {
                    yield return o;
                }
            }
        }
        
        [UnityTest]
        public IEnumerator HeapHead_Radix_Temp_Int()
        {
            var heap = new RadixHeap<int, IntValueConverter>();
            {
                var random = new Random();
                var prev = 0;
                foreach (var o in HeapHead(heap,
                    () => prev == int.MaxValue ? int.MaxValue : prev = random.Next(prev, int.MaxValue), 10))
                {
                    yield return o;
                }
            }
        }
        
        [UnityTest]
        public IEnumerator CheckSorted_NativeRadix_Temp_Int()
        {
            using (var heap = new NativeRadixHeap<int, IntValueConverter>(Allocator.Temp))
            {
                var random = new Random();
                var prev = 0;
                foreach (var o in CheckSorted(heap,
                    () => prev == int.MaxValue ? int.MaxValue : prev = random.Next(prev, int.MaxValue), 10))
                {
                    yield return o;
                }
            }
        }
        
        [UnityTest]
        public IEnumerator CheckSorted_NativeBinary_Temp_Int()
        {
            using (var heap = new NativeBinaryHeap<int>(Allocator.Persistent))
            {
                var random = new Random();
                foreach (var o in CheckSorted(heap, () => random.Next(), 100))
                {
                    yield return o;
                }
            }
        }
    }
}
