using System.Threading;

namespace BlueDove.UGraph
{
    public interface IIDHolder
    {
        int ID { get; }
    }

    public struct IDPublisherS
    {
        private int _count;

        public int Publish()
            => Interlocked.Increment(ref _count);
    }

    public class IDPublisher
    {
        private IDPublisherS b;
        public int Publish() => b.Publish();
    }
}