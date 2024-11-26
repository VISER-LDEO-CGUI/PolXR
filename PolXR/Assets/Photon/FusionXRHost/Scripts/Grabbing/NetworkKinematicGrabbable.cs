using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fusion.XR.Host.Grabbing
{
    /**
     * 
     * Declare that this game object can be grabbed by a NetworkGrabber
     * 
     * Handle following the grabbing NetworkGrabber
     * 
     **/
    [RequireComponent(typeof(KinematicGrabbable))]
    [DefaultExecutionOrder(NetworkKinematicGrabbable.EXECUTION_ORDER)]
    public class NetworkKinematicGrabbable : NetworkGrabbable
    {
        public new const int EXECUTION_ORDER = NetworkGrabber.EXECUTION_ORDER + 10;
        float ungrabResyncDuration = 2;

        KinematicGrabbable grabbable;

        private void Awake()
        {
            grabbable = GetComponent<KinematicGrabbable>();
        }

        #region NetworkGrabbable
        public override void Ungrab(NetworkGrabber grabber, GrabInfo newGrabInfo)
        {
            if (CurrentGrabber != grabber)
            {
                // This object as been grabbed by another hand, no need to trigger an ungrab
                return;
            }

            var lastGrabber = CurrentGrabber;
            CurrentGrabber = null;
            // We force the ungrab position at the one provided in the input, as the object has been ungrabbed between ticks, and so we shared this data to have subtick accurate ungrab position
            grabbable.transform.position = newGrabInfo.ungrabPosition;
            grabbable.transform.rotation = newGrabInfo.ungrabRotation;

            grabbable.DidUngrab();
            DidUngrab(lastGrabber);
        }

        public override void Grab(NetworkGrabber newGrabber, GrabInfo newGrabInfo)
        {
            grabbable.localPositionOffset = newGrabInfo.localPositionOffset;
            grabbable.localRotationOffset = newGrabInfo.localRotationOffset;
            grabbable.DidGrab();

            CurrentGrabber = newGrabber;
            DidGrab();
        }
        #endregion

        #region NetworkBehaviour
        public override void FixedUpdateNetwork()
        {
            // We only update the object position if we have the state authority
            if (!Object.HasStateAuthority) return;

            if (!IsGrabbed) return;
            // Follow grabber, adding position/rotation offsets
            grabbable.Follow(followingtransform: transform, followedTransform: CurrentGrabber.transform);
        }

        public override void Render()
        {
            if (Object.InputAuthority != Runner.LocalPlayer)
            {
                // Allow to prevent local hardware grabbing of the same object
                grabbable.isGrabbed = IsGrabbed;
            }
                
            if (IsGrabbed)
            {
                // Extrapolation: Make visual representation follow grabber visual representation, adding position/rotation offsets
                // We extrapolate for all users: we know that the grabbed object should follow accuratly the grabber, even if the network position might be a bit out of sync
                grabbable.Follow(followingtransform: transform, followedTransform: CurrentGrabber.transform);
            } 
            else if (grabbable.ungrabTime != -1)
            {
                if ((Time.time - grabbable.ungrabTime) < ungrabResyncDuration)
                {
                    // When the local user just ungrabbed the object, the network transform interpolation is still not the same as the extrapolation 
                    //  we were doing while the object was grabbed. So for a few frames, we need to ensure that the extrapolation continues
                    //  (ie. the object stay still)
                    //  until the network transform offers the same visual conclusion that the one we used to do
                    // Other ways to determine this extended extrapolation duration do exist (based on interpolation distance, number of ticks, ...)
                    transform.position = grabbable.ungrabPosition;
                    transform.rotation = grabbable.ungrabRotation;
                }
                else
                {
                    // We'll let the NetworkTransform do its normal interpolation again
                    grabbable.ungrabTime = -1;
                }
            }
        }
        #endregion
    }
}
