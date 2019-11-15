using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueDove.Sample
{
    public class MultiSelectRay : SelectRayBase
    {
        public event Action<RaycastHit[], int> HitAction;
        private RaycastHit[] _rayCache;

        public int CacheSize
        {
            get => _rayCache.Length;
            set => _rayCache = new RaycastHit[value];
        }

        private void Awake()
        {
            Click.performed += Selected;
            _rayCache = new RaycastHit[4];
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

        private void OnDestroy()
        {
            Click.performed -= Selected;
        }

        private void Selected(InputAction.CallbackContext context)
        {
            var point = Ray.ReadValue<Vector2>();
            var ray = rayCamera.ScreenPointToRay(point);
            var raycastHitCount = Physics.RaycastNonAlloc(ray, _rayCache, maxDistance);
            if (raycastHitCount > 0)
            {
                HitAction?.Invoke(_rayCache, raycastHitCount);
            }
        }
    }
}