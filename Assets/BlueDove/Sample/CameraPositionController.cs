using System;
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
        public InputAction zMover;
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
                pos += xyDiff;
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
                _camera.orthographicSize = Mathf.Max(1f, _camera.orthographicSize + f);
            }
        }
    }
}
