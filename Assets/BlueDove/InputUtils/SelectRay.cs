using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueDove.InputUtils
{
    public class SelectRay : MonoBehaviour
    {
        public InputAction click;
        public InputAction ray;

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
            click.performed += Selected;
            RaycastHitsCashSize = 4;
        }

        private void OnEnable()
        {
            click.Enable();
            ray.Enable();
        }

        private void OnDisable()
        {
            click.Disable();
            ray.Disable();
        }

        private void Selected(InputAction.CallbackContext context)
        {
            //Debug.Log("Clicked");
            var point = this.ray.ReadValue<Vector2>();
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