using Fusion.XR.Shared.Rig;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Fusion.XR.Shared.Desktop
{
    /***
     * 
     * MouseCamera computes the rotation of the head according to the mouse movements
     * 
     ***/
    public class MouseCamera : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        public InputActionProperty mouseXAction;
        public InputActionProperty mouseYAction;
#endif
        public bool forceRotation = false;

        public HardwareRig rig;
        [Header("Mouse point of view")]
        public Vector2 maxMouseInput = new Vector2(10, 10);
        public float maxHeadRotationSpeed = 30;
        public Vector2 sensitivity = new Vector2(10, 10);
        float referenceScreenWidth = 1920f;
        float referenceScreenHeight = 1080f;
        public float maxHeadAngle = 65;
        public float minHeadAngle = 65;
        Vector3 rotation = Vector3.zero;
        Vector2 mouseInput;

        Transform Head => rig == null ? null : rig.headset.transform;


        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            if (mouseXAction.action.bindings.Count == 0) mouseXAction.action.AddBinding("<Mouse>/delta/x");
            if (mouseYAction.action.bindings.Count == 0) mouseYAction.action.AddBinding("<Mouse>/delta/y");

            mouseXAction.action.Enable();
            mouseYAction.action.Enable();
#else
            Debug.LogError("Missing com.unity.inputsystem package");
#endif
            if (rig == null) rig = GetComponentInParent<HardwareRig>();
        }


        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (forceRotation || Mouse.current.rightButton.isPressed)
            {
                mouseInput.x = mouseXAction.action.ReadValue<float>() * Time.deltaTime * sensitivity.x * referenceScreenWidth / Screen.width;
                mouseInput.y = mouseYAction.action.ReadValue<float>() * Time.deltaTime * sensitivity.y * referenceScreenHeight / Screen.height;

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
#endif
        }
    }
}
