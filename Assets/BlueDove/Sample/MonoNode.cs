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
    public class MonoNode : MonoBehaviour, IIDHolder, IEquatable<MonoNode>, ICostFunc<MonoNode>, IMarkable
    {
        public int ID { get; private set; }

        internal void SetID(int id)
        {
            if (ID == 0) ID = id;
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
        {
            return ID;
        }

        public void Mark(Color color)
        {
            var mesh = GetComponent<MeshFilter>().mesh;
            var colors = new List<Color>(mesh.vertexCount);
            for (var i = 0; i < mesh.vertexCount; i++)
                colors.Add(color);
            mesh.SetColors(colors);
            mesh.UploadMeshData(false);
        }
    }
}