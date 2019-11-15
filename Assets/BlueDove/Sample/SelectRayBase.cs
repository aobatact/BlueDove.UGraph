using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace BlueDove.Sample
{
    public abstract class SelectRayBase : MonoBehaviour
    {
        public InputAction Click;
        public InputAction Ray;
        public Camera rayCamera;
        public float maxDistance = float.MaxValue;
    }
}