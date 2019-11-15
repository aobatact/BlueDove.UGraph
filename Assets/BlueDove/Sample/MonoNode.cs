using System;
using System.Collections.Generic;
using System.Diagnostics;
using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using UnityEngine;

namespace BlueDove.Sample
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MonoNode : MonoBehaviour, IIDHolder, IEquatable<MonoNode>, ICostFunc<MonoNode>, IMarkable, IDisposable, IVector3Node
    {
        private int _id;

        public int ID
        {
            get => _id;
            set
            {
                if(_id == 0)
                    _id = value;
                else
                    throw new InvalidOperationException();
            }
        }

        public bool Equals(MonoNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        public float Calc(MonoNode value) => Vector3.Distance(transform.position, value.transform.position);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MonoNode) obj);
        }

        public override int GetHashCode()
            => ID;

        public void Mark(Color color)
        {
            var mesh = GetComponent<MeshFilter>().mesh;
            var colors = new List<Color>(mesh.vertexCount);
            for (var i = 0; i < mesh.vertexCount; i++)
                colors.Add(color);
            mesh.SetColors(colors);
            mesh.UploadMeshData(false);
        }
        
        public void Dispose()
        {
            if (enabled)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            enabled = false;
            var graph = GetComponentInParent<MonoGraph>();
            if (graph != null)
                graph.RemoveNode(this);
            Destroy(gameObject);
        }

        public Vector3 Position => transform.position;
    }
}