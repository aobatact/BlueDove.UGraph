using System;
using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using BlueDove.UGraph.Mono;
using UnityEngine;

namespace BlueDove.SampleV2
{
    public class SampleNode : NodeBase, IEquatable<SampleNode>, ICostFunc<SampleNode>, IMarkable
    {
        private MeshFilter _filter;

        private void Awake() => _filter = GetComponent<MeshFilter>();
        public bool Equals(SampleNode other) => other != null && ID == other.ID;
        public float Calc(SampleNode value) => (value.Position - Position).magnitude;

        public void Mark(Color color)
        {
            var mesh = _filter.mesh;
            var colors = mesh.colors32;
            if (colors.Length == 0) colors = new Color32[4];
            var col32 = (Color32)color;
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = col32;
            }
            mesh.colors32 = colors;
        }
    }
}
