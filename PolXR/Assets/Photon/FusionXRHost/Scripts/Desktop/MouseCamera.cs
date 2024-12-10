using Fusion.XR.Host.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.XR.Host.Desktop
{
    public class MouseCamera : MonoBehaviour
    {
        public InputActionProperty mouseXAction;
        public InputActionProperty mouseYAction;

        public bool forceRotation = false;

        public HardwareRig rig;
        [Header("Mouse point of view")]
        public Vector2 maxMouseInput = new Vector2(10, 10);
        public float maxHeadRotationSpeed = 30;
        public Vector2 sensitivity = new Vector2(10, 10);
        public float maxHeadAngle = 65;
        public float minHeadAngle = 65;
        Vector3 rotation = Vector3.zero;
        Vector2 mouseInput;

        Transform Head => rig == null ? null : rig.headset.transform;


        private void Awake()
        {
            if (mouseXAction.action.bindings.Count == 0) mouseXAction.action.AddBinding("<Mouse>/delta/x");
            if (mouseYAction.action.bindings.Count == 0) mouseYAction.action.AddBinding("<Mouse>/delta/y");

            mouseXAction.action.Enable();
            mouseYAction.action.Enable();

            if (rig == null) rig = GetComponentInParent<HardwareRig>();
        }


        private void Update()
        {
            if (forceRotation || Mouse.current.rightButton.isPressed)
            {
                mouseInput.x = mouseXAction.action.ReadValue<float>() * Time.deltaTime * sensitivity.x;
                mouseInput.y = mouseYAction.action.ReadValue<float>() * Time.deltaTime * sensitivity.y;

                mouseInput.y = Mathf.Clamp(mouseInput.y, -maxMouseInput.y, maxMouseInput.y);
                mouseInput.x = Mathf.Clamp(mouseInput.x, -maxMouseInput.x, maxMouseInput.x);

                rotation.x = Head.eulerAngles.x - mouseInput.y;
                rotation.y = Head.eulerAngles.y + mouseInput.x;

                if (rotation.x > maxHeadAngle && rotation.x < (360 - minHeadAngle))
                {
                    if (Mathf.Abs(maxHeadAngle - rotation.x) < Mathf.Abs(rotation.x - (360 - minHeadAngle)))
                    {
                        rotation.x = maxHeadAngle;
                    }
                    else
                    {
                        rotation.x = -minHeadAngle;
                    }
                }
                else if (rotation.x < -minHeadAngle)
                {
                    rotation.x = -minHeadAngle;
                }

                var newRot = Quaternion.Lerp(Head.rotation, Quaternion.Euler(rotation), maxHeadRotationSpeed * Time.deltaTime);

                Head.rotation = newRot;
            }
        }
    }
}
