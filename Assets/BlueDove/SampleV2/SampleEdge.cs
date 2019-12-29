using System;
using BlueDove.UGraph.Mono;

namespace BlueDove.SampleV2
{
    public class SampleEdge : CachedLREdgeBase<SampleNode>, IEquatable<SampleEdge>
    {
        public bool Equals(SampleEdge other) 
            => other != null && (Source.Equals(other.Source) && Target.Equals(other.Target));

        new void Start()
        {
            base.Start();
        }

        /*　// Update if we can freely change node position
        private void Update()
        {
            ReDraw();
        }
        */
    }
}
