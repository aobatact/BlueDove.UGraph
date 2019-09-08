namespace BlueDove.UGraph.Algorithm
{
    public interface ICostFunc<in T>
    {
        float Calc(T value);
    }

    public struct DijkstraHeuristicFunc<T> : ICostFunc<T>
    {
        public float Calc(T value) => 0f;
    }
}