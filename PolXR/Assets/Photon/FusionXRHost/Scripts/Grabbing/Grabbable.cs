using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    public abstract class Grabbable : MonoBehaviour
    {
        public Grabber currentGrabber;
        [HideInInspector]
        public NetworkGrabbable networkGrabbable = null;
        [HideInInspector]
        public Vector3 localPositionOffset;
        [HideInInspector]
        public Quaternion localRotationOffset;
        [HideInInspector]
        public Vector3 ungrabPosition;
        [HideInInspector]
        public Quaternion ungrabRotation;
        [HideInInspector]
        public Vector3 ungrabVelocity;
        [HideInInspector]
        public Vector3 ungrabAngularVelocity;
        public abstract Vector3 Velocity { get; }

        public abstract Vector3 AngularVelocity { get; }

        public bool isGrabbed = false;

        public virtual bool Grab(Grabber newGrabber)
        {
            if (isGrabbed) return false;
            // Find grabbable position/rotation in grabber referential
            localPositionOffset = newGrabber.transform.InverseTransformPoint(transform.position);
            localRotationOffset = Quaternion.Inverse(newGrabber.transform.rotation) * transform.rotation;
            currentGrabber = newGrabber;
            isGrabbed = true;
            return true;
        }

        public virtual void Ungrab()
        {
            currentGrabber = null;
            if (networkGrabbable)
            {
                ungrabPosition = networkGrabbable.transform.position;
                ungrabRotation = networkGrabbable.transform.rotation;
                ungrabVelocity = Velocity;
                ungrabAngularVelocity = AngularVelocity;
            }
            isGrabbed = false;
        }
    }
}
