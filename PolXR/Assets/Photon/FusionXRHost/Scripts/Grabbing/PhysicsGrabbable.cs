using Fusion.XR.Host.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    public class PhysicsGrabbable : Grabbable
    {
        public Rigidbody rb;
        protected virtual void Awake()
        {
            networkGrabbable = GetComponent<NetworkGrabbable>();
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
        }
        public override Vector3 Velocity => rb.velocity;
        public override Vector3 AngularVelocity => rb.angularVelocity;
        public bool IsGrabbed => currentGrabber != null;

        #region Follow configuration        
        [Header("Follow configuration")]
        [Range(0, 1)]
        public float followVelocityAttenuation = 0.5f;
        public float maxVelocity = 10f;

        public enum FollowMode
        {
            Velocity,
            PID
        }
        public FollowMode followMode = FollowMode.Velocity;

        [Header("PID")]
        public PIDState pid = new PIDState { pidSettings = new PIDSettings {
            proportionalGain = 0.75f,
            integralGain = 0.01f,
            derivativeGain = 0.12f,
            maxIntegrationMagnitude = 10f
        } };
        public float commandScale = 1.5f;
        public float maxCommandMagnitude = 100f;
        public bool ignorePidIntegrationWhileColliding = true;
        #endregion

        bool isCollidingOffline = false;

        #region Following logic

        public virtual void Follow(Transform followedTransform, float elapsedTime, bool isColliding)
        {

             if (followMode == FollowMode.PID)
            {
                PIDFollow(followedTransform, elapsedTime, isColliding);
            }
            else if (followMode == FollowMode.Velocity)
            {
                VelocityFollow(followedTransform, elapsedTime);
            }
        }

        public virtual void PIDFollow(Transform followedTransform, float elapsedTime, bool isColliding)
        {
            var targetPosition = followedTransform.TransformPoint(localPositionOffset);
            var targetRotation = followedTransform.rotation * localRotationOffset;

            var error = targetPosition - rb.position;
            var ignoreIntegration = ignorePidIntegrationWhileColliding && isColliding;
            if (ignoreIntegration)
            {
                pid.errorIntegration = Vector3.zero;
            }
            var command = pid.UpdateCommand(error, elapsedTime, ignoreIntegration: ignoreIntegration);
            var impulse = Vector3.ClampMagnitude(commandScale * command, maxCommandMagnitude);
            rb.AddForce(impulse, ForceMode.Impulse);
            rb.angularVelocity = rb.transform.rotation.AngularVelocityChange(newRotation: targetRotation, elapsedTime: elapsedTime);
        }

        public virtual void VelocityFollow(Transform followedTransform, float elapsedTime)
        {
            // Compute the requested velocity to joined target position during a Runner.DeltaTime
            rb.VelocityFollow(target: followedTransform, localPositionOffset, localRotationOffset, elapsedTime);

            // To avoid a too aggressive move, we attenuate and limit a bit the expected velocity
            rb.velocity *= followVelocityAttenuation; // followVelocityAttenuation = 0.5F by default
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity); // maxVelocity = 10f by default
        }
        #endregion

        private void FixedUpdate()
        {
            // We handle the following if we are not online (in that case, the Follow will be called by the NetworkGrabbable during FUN and Render)
            if (networkGrabbable == null || networkGrabbable.Object == null)
            {
                // Note that this offline following will not offer the pseudo-haptic feedback (it could easily be recreated offline if needed)
                if (IsGrabbed) Follow(followedTransform: currentGrabber.transform, Time.fixedDeltaTime, isColliding: isCollidingOffline);
            }
            isCollidingOffline = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            isCollidingOffline = true;
        }

    }
}
