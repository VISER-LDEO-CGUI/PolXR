#define USE_PHYSICSADDON
#if USE_PHYSICSADDON
using Fusion.Addons.Physics;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR.Shared.Grabbing
{
    /**
     * 
     * Declare that this game object can be grabbed by a NetworkGrabber
     * 
     * Handle following the grabbing NetworkGrabber
     * 
     **/
    [DefaultExecutionOrder(NetworkGrabbable.EXECUTION_ORDER)]
    public class NetworkGrabbable : NetworkBehaviour
    {
        public const int EXECUTION_ORDER = NetworkGrabber.EXECUTION_ORDER + 10;
        [HideInInspector]
        public NetworkTransform networkTransform;
        public NetworkRigidbody3D networkRigidbody;
        [Networked]
        public NetworkBool InitialIsKinematicState { get; set; }
        [Networked]
        public NetworkGrabber CurrentGrabber { get; set; }
        [Networked]
        public Vector3 LocalPositionOffset { get; set; }
        [Networked]
        public Quaternion LocalRotationOffset { get; set; }

        public bool IsGrabbed => Object != null && CurrentGrabber != null; // We make sure that we are online before accessing [Networked] var


        [Header("Events")]
        public UnityEvent onDidUngrab = new UnityEvent();
        public UnityEvent<NetworkGrabber> onDidGrab = new UnityEvent<NetworkGrabber>();

        [Header("Advanced options")]
        public bool extrapolateWhileTakingAuthority = true;
        public bool isTakingAuthority = false;

        [HideInInspector]
        public Grabbable grabbable;
        ChangeDetector funChangeDetector;
        ChangeDetector renderChangeDetector;

        bool TryDetectGrabberChange(ChangeDetector changeDetector, out NetworkGrabber previousGrabber, out NetworkGrabber currentGrabber)
        {
            previousGrabber = null;
            currentGrabber = null;
            foreach (var changedNetworkedVarName in changeDetector.DetectChanges(this, out var previous, out var current))
            {
                if (changedNetworkedVarName == nameof(CurrentGrabber))
                {
                    var grabberReader = GetBehaviourReader<NetworkGrabber>(changedNetworkedVarName);
                    previousGrabber = grabberReader.Read(previous);
                    currentGrabber = grabberReader.Read(current);
                    return true;
                }
            }
            return false;
        }

        protected virtual void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            networkRigidbody = GetComponent<NetworkRigidbody3D>();
            grabbable = GetComponent<Grabbable>();
            if (grabbable == null)
            {
                Debug.LogError("NetworkGrabbable requires a Grabbable");// We do not use requireComponent as this classes can be subclassed
                gameObject.AddComponent<Grabbable>();
            }
        }

        #region Interface for local Grabbable (when the local user grab/ungrab this object)
        public virtual void LocalUngrab()
        {
            CurrentGrabber = null;
        }

        public async virtual void LocalGrab()
        {
            // Ask and wait to receive the stateAuthority to move the object
            isTakingAuthority = true;
            await Object.WaitForStateAuthority();
            isTakingAuthority = false;

            // We waited to have the state authority before setting Networked vars
            LocalPositionOffset = grabbable.localPositionOffset;
            LocalRotationOffset = grabbable.localRotationOffset;

            if(grabbable.currentGrabber==null)
            {
                // The grabbable has already been ungrabbed
                return;
            }
            // Update the CurrentGrabber in order to start following position in the FixedUpdateNetwork
            CurrentGrabber = grabbable.currentGrabber.networkGrabber;
        }
        #endregion

        public override void Spawned()
        {
            base.Spawned();

            // Save initial kinematic state for later join player
            if (Object.HasStateAuthority && grabbable.rb)
            {
                InitialIsKinematicState = grabbable.rb.isKinematic;
            }


            // We store the default kinematic state, while it is not affected by NetworkRigidbody logic
            grabbable.expectedIsKinematic = InitialIsKinematicState;

            funChangeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            renderChangeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
        }

        public override void FixedUpdateNetwork()
        {
            // Check if the grabber changed
            if (TryDetectGrabberChange(funChangeDetector, out var previousGrabber, out var currentGrabber))
            {
                if (previousGrabber)
                {
                    grabbable.UnlockObjectPhysics();
                }
                if (currentGrabber)
                {
                    grabbable.LockObjectPhysics();
                }
            }

            // We only update the object position if we have the state authority
            if (!Object.HasStateAuthority) return;

            if (!IsGrabbed) return;
            // Follow grabber, adding position/rotation offsets
            grabbable.Follow(followedTransform: CurrentGrabber.transform, LocalPositionOffset, LocalRotationOffset);
        }

        public override void Render()
        {
            // Check if the grabber changed, to trigger callbacks only (actual grabbing logic in handled in FUN for the state authority)
            // Those callbacks can't be called in FUN, as FUN is not called on proxies, while render is called for everybody
            if (TryDetectGrabberChange(renderChangeDetector, out var previousGrabber, out var currentGrabber))
            {
                if (previousGrabber)
                {
                    if (onDidUngrab != null) onDidUngrab.Invoke();
                }
                if (currentGrabber)
                {
                    if (onDidGrab != null) onDidGrab.Invoke(currentGrabber);
                }
            }

            if (isTakingAuthority && extrapolateWhileTakingAuthority)
            {
                // If we are currently taking the authority on the object due to a grab, the network info are still not set
                //  but we will extrapolate anyway (if the option extrapolateWhileTakingAuthority is true) to avoid having the grabbed object staying still until we receive the authority
                ExtrapolateWhileTakingAuthority();
                return;
            }

            // No need to extrapolate if the object is not grabbed
            if (!IsGrabbed) return;

            // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
            // We extrapolate for all users: we know that the grabbed object should follow accuratly the grabber, even if the network position might be a bit out of sync
            var follwedGrabberRoot = CurrentGrabber.hand != null ? CurrentGrabber.hand.gameObject : CurrentGrabber.gameObject;
            grabbable.Follow(followedTransform: follwedGrabberRoot.transform, LocalPositionOffset, LocalRotationOffset);
        }

        protected virtual void ExtrapolateWhileTakingAuthority()
        {
            // No need to extrapolate if the object is not really grabbed
            if (grabbable.currentGrabber == null) return;
            NetworkGrabber networkGrabber = grabbable.currentGrabber.networkGrabber;

            // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
            // We use grabberWhileTakingAuthority instead of CurrentGrabber as we are currently waiting for the authority transfer: the network vars are not already set, so we use the temporary versions
            var follwedGrabberRoot = networkGrabber.hand != null ? networkGrabber.hand.gameObject : networkGrabber.gameObject;
            grabbable.Follow(followedTransform: follwedGrabberRoot.transform, grabbable.localPositionOffset, grabbable.localRotationOffset);
        }
    }
}
