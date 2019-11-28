using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BlueDove.UGraph.Mono
{
    /// <inheritdoc cref="IEdge{TNode}"/>
    /// <summary>
    /// Base class for Edge which can Attach to GameObject
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    public abstract class EdgeBase<TNode> : MonoBehaviour, IEdge<TNode>, IEquatable<EdgeBase<TNode>>
        where TNode : IVector3Node, IEquatable<TNode>
    {
        public TNode Source { get; internal set; }
        public TNode Target { get; internal set; }

        public Vector3 Direction => Target.Position - Source.Position;

        public bool Equals(EdgeBase<TNode> other)
            => other != null && Source.Equals(other.Source) && Target.Equals(other.Target);
    }

    /// <inheritdoc cref="EdgeBase{TNode}"/>
    /// <summary>
    /// Base class for Edge Visible by LineRenderer
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    [RequireComponent(typeof(LineRenderer))]
    public abstract class LineRenderedEdgeBase<TNode> : EdgeBase<TNode> where TNode : IVector3Node, IEquatable<TNode>
    {
        [SerializeField] protected LineRenderer _renderer;
        public LineRenderer Renderer => _renderer;

        private void Start() => StartInner();

        protected void StartInner()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<LineRenderer>();
            }
            if (Renderer == null)
            {
                Debug.LogAssertion("No Renderer Attached");
            }
            Draw();
        }

        protected virtual void Draw()
        {
            var pos2 = new Vector3[2];
            pos2[0] = Source.Position;
            pos2[1] = Target.Position;
            Renderer.SetPositions(pos2);
            SetMeshToCollider();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetMeshToCollider()
        {
            var collider = GetComponent<MeshCollider>();
            if (collider != null)
            {
                SetMeshToCollider(collider);
            }
        }

        protected void SetMeshToCollider(MeshCollider collider)
        {
            var mesh = collider.sharedMesh ?? new Mesh();
            Renderer.BakeMesh(mesh);
            collider.sharedMesh = mesh;
        }
    }

    /// <inheritdoc cref="LineRenderedEdgeBase{TNode}"/>
    /// <summary>
    /// Base class for LineRendered Edge for which Node often moves
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public abstract class CachedLREdgeBase<TNode> : LineRenderedEdgeBase<TNode> where TNode : IVector3Node, IEquatable<TNode>
    {
        /// <summary>
        /// cache for no reallocate 
        /// </summary>
        private Vector3[] posCache;

        private MeshCollider _collider;
        
        protected void Start()
        {
            posCache = new Vector3[2];
            StartInner();
            _collider = GetComponent<MeshCollider>();
        }

        /// <summary>
        /// Re Draw Line Renderer after moving Node
        /// </summary>
        public void ReDraw() => Draw();

        protected override void Draw()
        {
            posCache[0] = Source.Position;
            posCache[1] = Target.Position;
            Renderer.SetPositions(posCache);
            if (_collider != null)
            {
                SetMeshToCollider(_collider);
            }
        }
    }
}
