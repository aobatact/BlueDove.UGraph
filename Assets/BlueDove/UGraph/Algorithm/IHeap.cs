namespace BlueDove.UGraph.Algorithm
{
    public interface IHeap<T>
    {
        int Count { get; }
        T Peek();
        T Pop();
        void Push(T value);
    }
    
    public interface IPriorityQueue<T> : IHeap<T>
    { }
}