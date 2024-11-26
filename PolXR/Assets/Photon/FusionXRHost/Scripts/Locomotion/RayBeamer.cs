using Fusion.XR.Host.Rig;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Fusion.XR.Host.Locomotion
{
    public struct RayData
    {
        public bool isRayEnabled;
        public Vector3 origin;
        public Vector3 target;
        public Color color;
    }

    /**
     * 
     * Display a line renderer when action input is pressed, and raycast other the selected layer mask to find a destination point
     * 
     **/

    public class RayBeamer : MonoBehaviour
    {
        public HardwareHand hand;

        public bool useRayActionInput = true;
        public InputActionProperty rayAction;
        public Transform origin;
        public LayerMask targetLayerMask = ~0;
        public float maxDistance = 100f;

        [Header("Representation")]
        public LineRenderer lineRenderer;
        public float width = 0.02f;
        public Material lineMaterial;

        public Color hitColor = Color.green;
        public Color noHitColor = Color.red;


        public UnityEvent<Collider, Vector3> onRelease = new UnityEvent<Collider, Vector3>();

        // Define if the beamer ray is active this frame
        public bool isRayEnabled = false;

        public enum Status
        {
            NoBeam,
            BeamNoHit,
            BeamHit
        }
        public Status status = Status.NoBeam;

        public RayData ray;
        Vector3 lastHit;
        Collider lastHitCollider = null;

        public virtual void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = lineMaterial;
                lineRenderer.numCapVertices = 4;
            }
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;

            if (origin == null) origin = transform;
            if (hand == null) hand = GetComponentInParent<HardwareHand>();
        }

        public virtual void Start()
        {
            rayAction.EnableWithDefaultXRBindings(hand.side, new List<string> { "thumbstickClicked", "primaryButton", "secondaryButton" });
        }

        public bool BeamCast(out RaycastHit hitInfo, Vector3 origin, Vector3 direction)
        {
            Ray handRay = new Ray(origin, direction);
            return Physics.Raycast(handRay, out hitInfo, maxDistance, targetLayerMask);
        }

        public bool BeamCast(out RaycastHit hitInfo)
        {
            return BeamCast(out hitInfo, ray.origin, origin.forward);
        }

        public void Update() {
            // If useRayActionInput is true, we read the rayAction to determine isRayEnabled for this frame
            //  Usefull for the mouse teleporter of the desktop mode, which disables the action reading to have its own logic to enable the beamer
            if (useRayActionInput && rayAction != null && rayAction.action != null)
            {
                isRayEnabled = rayAction.action.ReadValue<float>() == 1;
            }

            ray.isRayEnabled = isRayEnabled;
            if (ray.isRayEnabled)
            {
                ray.origin = origin.position;
                if (BeamCast(out RaycastHit hit))
                {
                    lastHitCollider = hit.collider;
                    ray.target = hit.point;
                    ray.color = hitColor;
                    lastHit = hit.point;
                    status = Status.BeamHit;
                }
                else
                {
                    lastHitCollider = null;
                    ray.target = ray.origin + origin.forward * maxDistance;
                    ray.color = noHitColor;
                    status = Status.BeamNoHit;
                }
            }
            else
            {
                if (status == Status.BeamHit)
                {
                    if (onRelease != null) onRelease.Invoke(lastHitCollider, lastHit);
                }
                status = Status.NoBeam;
                lastHitCollider = null;
            }

            UpdateRay();
        }

        public void CancelHit()
        {
            status = Status.NoBeam;
        }

        void UpdateRay() { 
            lineRenderer.enabled = ray.isRayEnabled;
            if (ray.isRayEnabled)
            {
                lineRenderer.SetPositions(new Vector3[] { ray.origin, ray.target });
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = ray.color;
                lineRenderer.endColor = ray.color;
            }
        }
    }
}
