using System;
using Unity.Mathematics;

namespace BlueDove.UGraph.Jobs
{
    public struct PositionNode2D : IEquatable<PositionNode2D>, IIDHolder
    {
        public float2 vec;
        public int id;

        public int ID => id;

        public PositionNode2D(float2 vec, int id)
        {
            this.vec = vec;
            this.id = id;
        }

        public bool Equals(PositionNode2D other)
            => vec.Equals(other.vec);

        public override bool Equals(object obj)
            => obj is PositionNode2D other && Equals(other);

        public override int GetHashCode()
            => ID;
    }
}