using System.Threading;

namespace BlueDove.UGraph
{
    public interface IIDHolder
    {
        int ID { get; }
    }

    internal struct IDPublisherBase
    {
        private int _count;

        public int Publish()
            => Interlocked.Increment(ref _count);
    }

    public class IDPublisher
    {
        private IDPublisherBase b;
        public int Publish() => b.Publish();
    }
}