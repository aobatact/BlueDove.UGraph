using System;
using BlueDove.UGraph.Mono;
using UnityEngine;

namespace BlueDove.SampleV2
{
    public class SampleEdge : VectorCacheLREdgeBase<SampleNode>, IEquatable<SampleEdge>
    {
        public bool Equals(SampleEdge other) 
            => other != null && (Source.Equals(other.Source) && Target.Equals(other.Target));

        new void Start()
        {
            base.Start();
        }
    }
}
