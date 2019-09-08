namespace BlueDove.UGraph.Algorithm
{
    public interface ICostFunc<in T>
    {
        float Calc(T value);
    }

    public interface IHeuristicFunc<in T>
    {
        float CalcHeuristic(T value);
    }
}