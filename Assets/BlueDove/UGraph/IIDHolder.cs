using System.Collections.Generic;
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

    public struct IDComparer<TID> : IComparer<TID> where TID : IIDHolder
    {
        public int Compare(TID x, TID y) => (x?.ID ?? 0).CompareTo(y?.ID ?? 0);
    }
    
    public struct IDComparerS<TID> : IComparer<TID> where TID : struct, IIDHolder
    {
        public int Compare(TID x, TID y) => x.ID.CompareTo(y.ID );
    }

}