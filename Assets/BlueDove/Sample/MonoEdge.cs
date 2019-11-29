using System;
using BlueDove.UGraph;
using UnityEngine;
using UnityEngine.Serialization;

namespace BlueDove.Sample
{
    [RequireComponent(typeof(LineRenderer))]
    public class MonoEdge : MonoBehaviour, IEdge<MonoNode>, IEquatable<MonoEdge>
    {
        private LineRenderer _renderer;
        private Vector3[] pos;
        private MeshCollider _collider;
#pragma warning disable 0649
        [SerializeField] private MonoNode source;
        [SerializeField] private MonoNode target;
#pragma warning restore 0649
        public LineRenderer Renderer => _renderer;
        
        // Start is called before the first frame update
        void Start()
        {
            if (_renderer == null)
                _renderer = GetComponent<LineRenderer>();
            if (_collider == null) 
                _collider = GetComponent<MeshCollider>();
            pos = new Vector3[2];
            ReDraw();
        }

        void Update()
        {
            if (!(_renderer is null) && (pos[0] != Source.transform.position || pos[1] != Target.transform.position))
            {
                ReDraw();
            }
        }

        private void ReDraw()
        {
            if (!(Source is null) && !(Target is null))
            {
                pos[0] = Source.transform.position;
                pos[1] = Target.transform.position;
                _renderer.SetPositions(pos);
                if (!(_collider is null))
                    SetMesh2Collider();
            }
            else
            {
                Debug.LogAssertion("Empty Node");
            }
        }

        private void SetMesh2Collider()
        {
            var mesh = _collider.sharedMesh ?? new Mesh();
            _renderer.BakeMesh(mesh);
            _collider.sharedMesh = mesh;
        }
        
        // Update is called once per frame

        public MonoNode Source
        {
            get => source;
            set => source = value;
        }

        public MonoNode Target
        {
            get => target;
            set => target = value;
        }

        public bool Equals(MonoEdge other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return (Source.Equals(other.Source) && Target.Equals(other.Target)) || 
                   (Source.Equals(other.Target) && Target.Equals(other.Source));
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MonoEdge) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Target != null ? Target.GetHashCode() : 0);
                return hashCode;
            }
        }

        public float GetDistanceSqr()
        {
            return Vector3.SqrMagnitude(Direction);
        }

        public Vector3 Direction => Source.transform.position - Target.transform.position;

        private void OnDestroy()
        {
            Destroy(gameObject);
        }
    }
}
