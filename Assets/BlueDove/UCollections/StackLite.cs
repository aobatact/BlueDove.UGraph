using System;

namespace BlueDove.UCollections
{
    public struct StackLite<T>
    {
        private T[] values;
        public int Count { get; private set; }

        public T[] Values
        {
            get => values;
        }

        public void Push(T value)
        {
            if (values.Length == Count)
            {
                Array.Resize(ref values,Count << 1);
            }
            values[Count++] = value;
        }

        public T Pop()
        {
            return values[--Count];
        }

        public void Clear() => Count = 0;
    }
}
