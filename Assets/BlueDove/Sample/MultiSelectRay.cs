using System;
using BlueDove.Samples;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueDove.Sample
{
    public class MultiSelectRay : IDisposable
    {
        public MultiSelectRay(UGraphSampleInputActions.SelectorActions actions, Camera rayCamera, float maxDistance  = float.MaxValue, int cacheSize = 1)
        {
            _actions = actions;
            this.rayCamera = rayCamera;
            this.maxDistance = maxDistance;
            _rayCache = new RaycastHit[cacheSize];
            _actions.Click.performed += Selected;
            _rayCache = new RaycastHit[4];
        }

        private readonly Camera rayCamera;
        public event Action<RaycastHit[], int> HitAction;
        private RaycastHit[] _rayCache;
        public float maxDistance;
        private UGraphSampleInputActions.SelectorActions _actions;
        
        public int CacheSize
        {
            get => _rayCache.Length;
            set => _rayCache = new RaycastHit[value];
        }

        public void Enable() => _actions.Enable();

        public void Disable() => _actions.Disable();

        private void Selected(InputAction.CallbackContext context)
        {
            var point = _actions.Ray.ReadValue<Vector2>();
            var ray = rayCamera.ScreenPointToRay(point);
            var raycastHitCount = Physics.RaycastNonAlloc(ray, _rayCache, maxDistance);
            if (raycastHitCount > 0)
            {
                HitAction?.Invoke(_rayCache, raycastHitCount);
            }
        }

        public void Dispose() => _actions.Click.performed -= Selected;
    }
}