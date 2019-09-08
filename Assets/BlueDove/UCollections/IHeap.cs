namespace BlueDove.Collections.Heaps
{
    public interface IHeap<T>
    {
        void Push(T value);
        T Peek();
        T Pop();
        bool TryPeek(out T value);
        bool TryPop(out T value);
        int Count { get; }
        void Clear();
    }
}