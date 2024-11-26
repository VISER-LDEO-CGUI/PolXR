using Fusion.XR.Host.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    public class Grabber : MonoBehaviour
    {
        public Grabbable grabbedObject;

        Collider lastCheckedCollider;
        Grabbable lastCheckColliderGrabbable;

        HardwareHand hand;
        HardwareRig rig;

        public Vector3 ungrabPosition;
        public Quaternion ungrabRotation; 
        public Vector3 ungrabVelocity;
        public Vector3 ungrabAngularVelocity;
        // Will be set by the NetworkGrabber for the local user itself, when it spawns
        public NetworkGrabber networkGrabber;

        public bool resetGrabInfo = false;

        GrabInfo _grabInfo;
        public GrabInfo GrabInfo
        {
            get
            {
                if (resetGrabInfo)
                    return default;

                if (grabbedObject)
                {
                    _grabInfo.grabbedObjectId = grabbedObject.networkGrabbable.Id;
                    _grabInfo.localPositionOffset = grabbedObject.localPositionOffset;
                    _grabInfo.localRotationOffset = grabbedObject.localRotationOffset;

                } 
                else
                {
                    _grabInfo.grabbedObjectId = NetworkBehaviourId.None;
                    _grabInfo.ungrabPosition = ungrabPosition;
                    _grabInfo.ungrabRotation = ungrabRotation; 
                    _grabInfo.ungrabVelocity = ungrabVelocity;
                    _grabInfo.ungrabAngularVelocity = ungrabAngularVelocity;
                }

                return _grabInfo;
            }
        }

        private void Awake()
        {
            hand = GetComponentInParent<HardwareHand>();
            rig = GetComponentInParent<HardwareRig>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (rig && rig.runner && rig.runner.IsResimulation)
            {
                // We only manage grabbing during forward ticks, to avoid detecting past positions of the grabbable object
                return;
            }

            // Exit if an object is already grabbed
            if (grabbedObject != null)
            {
                // It is already the grabbed object or another, but we don't allow shared grabbing here
                return;
            }

            Grabbable grabbable;

            if (lastCheckedCollider == other)
            {
                grabbable = lastCheckColliderGrabbable;
            }
            else
            {
                grabbable = other.GetComponentInParent<Grabbable>();
            }
            // To limit the number of GetComponent calls, we cache the latest checked collider grabbable result
            lastCheckedCollider = other;
            lastCheckColliderGrabbable = grabbable;
            if (grabbable != null)
            {
                if (grabbable.currentGrabber != null)
                {
                    // We don't allow multihand grabbing (it would have to be defined), nor hand swap (it would require to track hovering and do not allow grabbing while the hand is already close - or any other mecanism to avoid infinit swapping between the hands)
                    return;
                }
                if (hand.isGrabbing) Grab(grabbable);
            }
        }

        private void Update()
        {
            if (rig && rig.runner && rig.runner.IsResimulation)
            {
                // We only manage grabbing during forward ticks, to avoid detecting past positions of the grabbable object
                return;
            }

            if (grabbedObject != null && grabbedObject.currentGrabber != this)
            {
                // This object as been grabbed by another hand, no need to trigger an ungrab
                grabbedObject = null;
            }


            // Check if the local hand is still grabbing the object
            if (grabbedObject != null && !hand.isGrabbing)
            {
                // Object released by this hand
                Ungrab(grabbedObject);
            }
        }

        // Ask the grabbable object to start following the hand
        public void Grab(Grabbable grabbable)
        {
            Debug.Log($"Try to grab object {grabbable.gameObject.name} with {gameObject.name}");
            if (grabbable.Grab(this))
            {
                grabbedObject = grabbable;
            }
        }

        // Ask the grabbable object to stop following the hand
        public void Ungrab(Grabbable grabbable)
        {
            Debug.Log($"Try to ungrab object {grabbable.gameObject.name} with {gameObject.name}");
            if (grabbable.networkGrabbable)
            {
                ungrabPosition = grabbedObject.networkGrabbable.transform.position;
                ungrabRotation = grabbedObject.networkGrabbable.transform.rotation;
                ungrabVelocity = grabbedObject.Velocity;
                ungrabAngularVelocity = grabbedObject.AngularVelocity;
            }

            grabbedObject.Ungrab();
            grabbedObject = null;
        }
    }
}
