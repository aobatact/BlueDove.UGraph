using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueDove.InputUtils
{
    public class SelectRay : MonoBehaviour
    {
        public InputAction Click;
        public InputAction Ray;

        public Camera rayCamera;
        public event Action<RaycastHit[], int> HitsAction;
        private RaycastHit[] hits;
        [SerializeField] private float maxDistance = 30;

        public int RaycastHitsCashSize
        {
            get => hits?.Length ?? 0;
            set => hits = new RaycastHit[value];
        }

        private void Awake()
        {
            Click.performed += Selected;
            RaycastHitsCashSize = 4;
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
            if (HitsAction != null)
            {
                var hitCount = Physics.RaycastNonAlloc(ray, hits, maxDistance);
                if (hitCount > 0)
                {
                    //Debug.Log("Hit");
                    HitsAction.Invoke(hits, hitCount);
                }
            }
        }
    }
}