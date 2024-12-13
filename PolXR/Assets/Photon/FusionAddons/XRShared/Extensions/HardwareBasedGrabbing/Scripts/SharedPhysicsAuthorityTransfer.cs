using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Fusion.XR.Shared.Grabbing {
    [DefaultExecutionOrder(SharedPhysicsAuthorityTransfer.EXECUTION_ORDER)]
    [RequireComponent(typeof(NetworkGrabbable))]
    /**
     * Component to allow proxy physics and authority transfer on collision, in shared topology
     *
     * Note: for advanced physics interaction, switching to host topology would be more interesting results
     */
    public class SharedPhysicsAuthorityTransfer : NetworkBehaviour, IBeforeTick
    {
        public const int EXECUTION_ORDER = NetworkRig.EXECUTION_ORDER;
        public bool IsGrabbed => grabbable.IsGrabbed;
        NetworkGrabbable grabbable;

        [Tooltip("If true, proxies won't be forced to have kinematic rigidbodies")]
        public bool allowProxyPhysics = true;
        [Tooltip("If true, when colliding an object not grabbed that is not owned (ie on which we have authority), we will request the authority")]
        public bool transferOwnershipOnCollision = true;

        Rigidbody rb;


        private void Awake()
        {
            grabbable = GetComponent<NetworkGrabbable>();
            rb = GetComponent<Rigidbody>();
        }

        public override void FixedUpdateNetwork()
        {
            if (rb && allowProxyPhysics && !IsGrabbed) rb.isKinematic = grabbable.grabbable.expectedIsKinematic;
        }

        void IBeforeTick.BeforeTick()
        {
            if (rb && allowProxyPhysics && !IsGrabbed) rb.isKinematic = grabbable.grabbable.expectedIsKinematic;
        }

        float lastAuthorityTakeOver = -1;
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!transferOwnershipOnCollision) return;
            if (!Object || !Object.HasStateAuthority) return;
            if (!collision.rigidbody) return;
            // Security to avoid multiple rapid exchange of authority (when both user think they should take authority)
            if ((Time.time - lastAuthorityTakeOver) < 1) return;

            // Request authority on collided object
            var other = collision.rigidbody.GetComponent<SharedPhysicsAuthorityTransfer>();
            // If we have authority on the current object, we check if we already have authority on the collided object
            if (other && !other.Object.HasStateAuthority && !other.IsGrabbed)
            {
                if (IsGrabbed || (rb.velocity.magnitude > collision.rigidbody.velocity.magnitude))
                {
                    lastAuthorityTakeOver = Time.time;
                    other.lastAuthorityTakeOver = Time.time;
                    other.Object.RequestStateAuthority();
                }
            }
        }
    }

}
