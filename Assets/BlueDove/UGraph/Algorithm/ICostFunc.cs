namespace BlueDove.UGraph.Algorithm
{
    public interface ICostFunc<in T>
    {
        float Calc(T value);
    }
}