using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    public class KinematicGrabbable : Grabbable
    {
        [HideInInspector]
        public float ungrabTime = -1;

        public bool IsGrabbed => currentGrabber != null;
        public override Vector3 Velocity => Vector3.zero;

        public override Vector3 AngularVelocity => Vector3.zero;

        private void Awake()
        {
            networkGrabbable = GetComponent<NetworkGrabbable>();
        }

        public override void Ungrab()
        {
            base.Ungrab();
            if (networkGrabbable)
            {
                // When networked, we need to store ungrab info detailled position for some extrapolation of edge cases
                ungrabTime = Time.time;
            }
            DidUngrab();
        }

        public override bool Grab(Grabber newGrabber)
        {
            if (base.Grab(newGrabber))
            {
                DidGrab();
                return true;
            }
            return false;
        }

        public void Follow(Transform followingtransform, Transform followedTransform)
        {
            followingtransform.position = followedTransform.TransformPoint(localPositionOffset);
            followingtransform.rotation = followedTransform.rotation * localRotationOffset;
        }

        private void Update()
        {
            // We handle the following if we are not online (in that case, the Follow will be called by the NetworkGrabbable during FUN and Render)
            if (networkGrabbable == null || networkGrabbable.Object == null)
            {
                if(IsGrabbed) Follow(followingtransform: transform, followedTransform: currentGrabber.transform);
            }
        }

        public void DidGrab()
        {
            // If anybody grabbed this object, we reset this last ungrab timestamp, which allows some graphical extrapolation
            ungrabTime = -1;
        }

        public void DidUngrab()
        {
        }
    }
}

