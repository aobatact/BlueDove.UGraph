﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueDove.Sample
{
    [RequireComponent(typeof(Camera))]
    public class CameraPositionController : MonoBehaviour
    {
        public InputAction XYMover;
        private Vector3 xyDiff;
        private bool xyChanged;
        private float speedSq = 1/3f;

        public InputAction zMover;
        public float minZoomSize = 1f;
        private float _currentZoomSizeSq;
        private Camera _camera;
        
        private void Awake()
        {
            XYMover.started += ChangeMoveXY;
            XYMover.canceled += StopXY;
            XYMover.performed += StopXY;
            zMover.performed += MoveZ;
        }

        private void Start()
        {
            _camera = GetComponent<Camera>();
            _currentZoomSizeSq = Mathf.Sqrt(_camera.orthographicSize) * speedSq;
        }

        private void OnEnable()
        {
            XYMover.Enable();
            zMover.Enable();
        }

        private void OnDisable()
        {
            XYMover.Disable();
            zMover.Disable();
        }


        void ChangeMoveXY(InputAction.CallbackContext context)
        {
            xyDiff = context.ReadValue<Vector2>();
            if (!xyChanged)
            {
                xyChanged = true;
                StartCoroutine(MoveXY().GetEnumerator());
            }
        }

        void StopXY(InputAction.CallbackContext context)
        {
            xyChanged = (xyDiff = context.ReadValue<Vector2>()) != Vector3.zero;
        }

        IEnumerable MoveXY()
        {
            while (xyChanged)
            {
                var t = transform;
                var pos = t.position;
                pos += xyDiff * _currentZoomSizeSq;
                t.position = pos;
                //yield return new WaitForSeconds(1/62f);
                yield return null;
            }
        }

        void MoveZ(InputAction.CallbackContext context)
        {
            var f = context.ReadValue<float>();
            if (f != 0)
            {
                var zoomSizeSq = _currentZoomSizeSq / speedSq;
                var currentZoomSize = zoomSizeSq * zoomSizeSq;
                var n = currentZoomSize + f;
                if (n < minZoomSize)
                {
                    if(currentZoomSize == minZoomSize)
                        return;
                    n = minZoomSize;
                    _currentZoomSizeSq = Mathf.Sqrt(minZoomSize) * speedSq;
                }
                else
                {
                    _currentZoomSizeSq = Mathf.Sqrt(n) * speedSq;
                }
                _camera.orthographicSize = n;
            }
        }
    }
}