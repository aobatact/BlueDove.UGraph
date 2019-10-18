using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueDove.Sample
{
    public class SelectRay : MonoBehaviour
    {
        public InputAction Click;
        public InputAction Ray;

        public Camera rayCamera;
        public event Action<RaycastHit> HitAction;
    
        private void Awake()
        {
            Click.performed += Selected;
        }

        private void OnEnable()
        {
            Click.Enable();
            Ray.Enable();
        }

        private void OnDisable()
        {
            Click.Disable();
            Ray.Disable();
        }

        private void Selected(InputAction.CallbackContext context)
        {
           //Debug.Log("Clicked");
            var point = Ray.ReadValue<Vector2>();
            var ray = rayCamera.ScreenPointToRay(point);
            if (Physics.Raycast(ray, out var hit))
            {
                //Debug.Log("Hit");
                HitAction?.Invoke(hit);
            }
        }
    }
}
