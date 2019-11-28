using System;
using BlueDove.UGraph.Mono;
using UnityEngine;

namespace BlueDove.SampleV2
{
    public class SampleNode : NodeBase, IEquatable<SampleNode>
    {
        public bool Equals(SampleNode other) => other != null && ID == other.ID;
    }
}
