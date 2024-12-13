using Fusion.XR.Shared.Locomotion;
using Fusion.XR.Shared.Rig;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Fusion.XR.Shared.Desktop
{
    /**
     * Desktop hardware movements controller, using keyboard to move
     * 
     * Movement is validated:
     * - by XRHardwareRig ILocomotionValidator
     * - by a check that we are on a valid NavMesh position
     * - by a check that the head won't enter in a collider
     */
    public class DesktopController : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        public InputActionProperty forwardAction;
        public InputActionProperty rotationAction;
#endif
        public HardwareRig rig;
        protected RigLocomotion locomotion;

        public float strafeSpeed = 3;
        public float forwardSpeed = 3;
        public float rotationSpeed = 180;

        private void Awake()
        {
            locomotion = GetComponent<RigLocomotion>();
        }

        void Start()
        {
#if ENABLE_INPUT_SYSTEM

            if (forwardAction != null && forwardAction.action != null)
            {
                if (forwardAction.reference == null && forwardAction.action.bindings.Count == 0)
                {
                    forwardAction.action.AddCompositeBinding("2DVector")
                        .With("Up", "<Keyboard>/w")
                        .With("Down", "<Keyboard>/s")
                        .With("Left", "<Keyboard>/a")
                        .With("Right", "<Keyboard>/d");
                }

                forwardAction.action.Enable();
            }
            if (rotationAction != null && rotationAction.action != null)
            {
                if (rotationAction.reference == null && rotationAction.action.bindings.Count == 0)
                {
                    rotationAction.action.AddCompositeBinding("Axis")
                        .With("Positive", "<Keyboard>/e")
                        .With("Negative", "<Keyboard>/q");
                }
                rotationAction.action.Enable();
            }
#else
            Debug.LogError("Missing com.unity.inputsystem package");
#endif
            rig = GetComponentInParent<HardwareRig>();
        }

        void Update()
        {
            float groundYPosition = ProjectHeadsetGroundPosition().y;
#if ENABLE_INPUT_SYSTEM
            if (rotationAction != null && rotationAction.action != null)
            {
                rig.Rotate(rotationAction.action.ReadValue<float>() * Time.deltaTime * rotationSpeed);
            }

            if (forwardAction != null && forwardAction.action != null)
            {
                var command = forwardAction.action.ReadValue<Vector2>();
                if ((Mouse.current.rightButton.isPressed && Mouse.current.leftButton.isPressed))
                {
                    command = new Vector2(command.x, 1);
                }
                if (command.magnitude == 0) return;
                var headsetMove = command.y * forwardSpeed * Time.deltaTime * rig.headset.transform.forward + command.x * strafeSpeed * Time.deltaTime * rig.headset.transform.right;
                var move = new Vector3(headsetMove.x, groundYPosition - rig.transform.position.y, headsetMove.z);
                var newPosition = rig.transform.position + move;

                Move(newPosition);
            }
#endif
        }

        public virtual void Move(Vector3 newPosition)
        {
            rig.Teleport(newPosition);
        }

        Vector3 ProjectHeadsetGroundPosition()
        {
            Vector3 groundPosition = rig.transform.position;
            if (Physics.Raycast(rig.headset.transform.position, -transform.up, hitInfo: out RaycastHit hit, maxDistance: 10, layerMask: locomotion.locomotionLayerMask))
            {
                if (locomotion.ValidLocomotionSurface(hit.collider))
                {
                    groundPosition = hit.point;
                }
            }
            return groundPosition;
        }
    }
}
