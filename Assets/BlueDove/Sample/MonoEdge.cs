﻿using System;
using BlueDove.UGraph;
using UnityEngine;
using UnityEngine.Serialization;

namespace BlueDove.Sample
{
    [RequireComponent(typeof(LineRenderer))]
    public class MonoEdge : MonoBehaviour, IEdge<MonoNode>, IEquatable<MonoEdge>
    {
        private LineRenderer _renderer;
        [SerializeField] private MonoNode source;
        [SerializeField] private MonoNode target;
        private Vector3[] pos;

        public LineRenderer Renderer => _renderer;
        
        // Start is called before the first frame update
        void Start()
        {
            if (_renderer == null)
                _renderer = GetComponent<LineRenderer>();
            pos = new Vector3[2];
            ReDraw();
        }

        void Update()
        {
            //var pos = new Vector3[2];
            //_renderer.GetPositions(pos);
            if (!(pos[0] == Source.transform.position) || !(pos[1] == Target.transform.position))
            {
                ReDraw();
            }
        }

        private void ReDraw()
        {
            if (Source != null && Target != null)
            {
                pos[0] = Source.transform.position;
                pos[1] = Target.transform.position;
                _renderer.SetPositions(pos);
            }
            else
            {
                Debug.LogAssertion("Empty Node");
            }
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
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (Source.Equals(other.Source) && Target.Equals(other.Target)) || 
                   (Source.Equals(other.Target) && Target.Equals(other.Source));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
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
