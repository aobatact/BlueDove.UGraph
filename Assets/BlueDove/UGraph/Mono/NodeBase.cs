using System;
using UnityEngine;

namespace BlueDove.UGraph.Mono
{
    public abstract class NodeBase : MonoBehaviour, IIDHolder, IEquatable<NodeBase>, IVector3Node
    {
        public int ID { get; internal set; }

        public bool Equals(NodeBase other) 
            => other != null && ID.Equals(other.ID);

        public Vector3 Position => transform.position;
    }

}
