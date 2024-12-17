#define USE_PHYSICSADDON
#if USE_PHYSICSADDON
using Fusion.Addons.Physics;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR.Shared.Grabbing.NetworkHandColliderBased
{
    /**
     * 
     * Declare that this game object can be grabbed by a NetworkHandColliderGrabber
     * 
     * Handle following the grabbing NetworkHandColliderGrabber
     * 
     **/

    [DefaultExecutionOrder(NetworkHandColliderGrabbable.EXECUTION_ORDER)]
    public class NetworkHandColliderGrabbable : NetworkBehaviour
    {
        public const int EXECUTION_ORDER = NetworkHandColliderGrabber.EXECUTION_ORDER + 10;
        [HideInInspector]
        public NetworkTransform networkTransform;
        public NetworkRigidbody3D networkRigidbody;
        [Networked]
        public NetworkBool InitialIsKinematicState { get; set; }
        [Networked]
        public NetworkHandColliderGrabber CurrentGrabber { get; set; }
        [Networked]
        private Vector3 LocalPositionOffset { get; set; }
        [Networked]
        private Quaternion LocalRotationOffset { get; set; }

        public bool IsGrabbed => CurrentGrabber != null;
        public bool expectedIsKinematic = true;
        [Tooltip("For object with a rigidbody, if true, apply hand velocity on ungrab")]
        public bool applyVelocityOnRelease = true;

        // Velocity computation
        const int velocityBufferSize = 5;
        Vector3 lastPosition;
        Quaternion previousRotation;
        Vector3[] lastMoves = new Vector3[velocityBufferSize];
        Vector3[] lastAngularVelocities = new Vector3[velocityBufferSize];
        float[] lastDeltaTime = new float[velocityBufferSize];
        int lastMoveIndex = 0;
        ChangeDetector funChangeDetector;
        ChangeDetector renderChangeDetector;

        [Header("Events")]
        public UnityEvent onDidUngrab = new UnityEvent();
        [Tooltip("Called only for the local grabber, when they may wait for authority before grabbing. onDidGrab will be called on all users")]
        public UnityEvent<NetworkHandColliderGrabber> onWillGrab = new UnityEvent<NetworkHandColliderGrabber>();
        public UnityEvent<NetworkHandColliderGrabber> onDidGrab = new UnityEvent<NetworkHandColliderGrabber>();

        [Header("Advanced options")]
        public bool extrapolateWhileTakingAuthority = true;
        public bool isTakingAuthority = false;

        Vector3 localPositionOffsetWhileTakingAuthority;
        Quaternion localRotationOffsetWhileTakingAuthority;
        NetworkHandColliderGrabber grabberWhileTakingAuthority;

        enum Status { 
            NotGrabbed,
            Grabbed,
            WillBeGrabbedUponAuthorityReception
        }
        Status status = Status.NotGrabbed;

        Vector3 Velocity
        {
            get
            {
                Vector3 move = Vector3.zero;
                float time = 0;
                for (int i = 0; i < velocityBufferSize; i++)
                {
                    if (lastDeltaTime[i] != 0)
                    {
                        move += lastMoves[i];
                        time += lastDeltaTime[i];
                    }
                }
                if (time == 0) return Vector3.zero;
                return move / time;
            }
        }

        Vector3 AngularVelocity
        {
            get
            {
                Vector3 culmulatedAngularVelocity = Vector3.zero;
                int step = 0;
                for (int i = 0; i < velocityBufferSize; i++)
                {
                    if (lastDeltaTime[i] != 0)
                    {
                        culmulatedAngularVelocity += lastAngularVelocities[i];
                        step++;
                    }
                }
                if (step == 0) return Vector3.zero;
                return culmulatedAngularVelocity / step;
            }
        }

        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            networkRigidbody = GetComponent<NetworkRigidbody3D>();

        }

        public override void Spawned()
        {
            base.Spawned();
            if (networkRigidbody && Object.HasStateAuthority)
            {
                // Save initial kinematic state for later join player
                InitialIsKinematicState = networkRigidbody.Rigidbody.isKinematic;
            }
            funChangeDetector = GetChangeDetector(NetworkBehaviour.ChangeDetector.Source.SimulationState);
            renderChangeDetector = GetChangeDetector(NetworkBehaviour.ChangeDetector.Source.SnapshotFrom);
        }

        public void Ungrab()
        {
            status = Status.NotGrabbed;
            if (Object.HasStateAuthority)
            {
                CurrentGrabber = null;
            }
        }

        public async void Grab(NetworkHandColliderGrabber newGrabber)
        {
            if (onWillGrab != null) onWillGrab.Invoke(newGrabber);

            // Find grabbable position/rotation in grabber referential
            localPositionOffsetWhileTakingAuthority = newGrabber.transform.InverseTransformPoint(transform.position);
            localRotationOffsetWhileTakingAuthority = Quaternion.Inverse(newGrabber.transform.rotation) * transform.rotation;
            grabberWhileTakingAuthority = newGrabber;

            // Ask and wait to receive the stateAuthority to move the object
            status = Status.WillBeGrabbedUponAuthorityReception;
            isTakingAuthority = true;
            await Object.WaitForStateAuthority();
            isTakingAuthority = false;
            if (status == Status.NotGrabbed)
            {
                // Object has been already ungrabbed while waiting for state authority
                return;
            }
            if (Object.HasStateAuthority == false)
            {
                Debug.LogError("Unable to receive state authority");
                return;
            }
            status = Status.Grabbed;

            // We waited to have the state authority before setting Networked vars
            LocalPositionOffset = localPositionOffsetWhileTakingAuthority;
            LocalRotationOffset = localRotationOffsetWhileTakingAuthority;

            // Update the CurrentGrabber in order to start following position in the FixedUpdateNetwork
            CurrentGrabber = grabberWhileTakingAuthority;
        }

        void LockObjectPhysics()
        {
            // While grabbed, we disable physics forces on the object, to force a position based tracking
            if (networkRigidbody) networkRigidbody.Rigidbody.isKinematic = true;
        }

        void UnlockObjectPhysics()
        {
            // We restore the default isKinematic state if needed
            if (networkRigidbody) networkRigidbody.Rigidbody.isKinematic = InitialIsKinematicState;

            // We apply release velocity if needed
            if (networkRigidbody && networkRigidbody.Rigidbody.isKinematic == false && applyVelocityOnRelease)
            {
                networkRigidbody.Rigidbody.velocity = Velocity;
                networkRigidbody.Rigidbody.angularVelocity = AngularVelocity;
            }

            // Reset velocity tracking
            for (int i = 0; i < velocityBufferSize; i++) lastDeltaTime[i] = 0;
            lastMoveIndex = 0;
        }

        bool TryDetectGrabberChange(ChangeDetector changeDetector, out NetworkHandColliderGrabber previousGrabber, out NetworkHandColliderGrabber currentGrabber)
        {
            previousGrabber = null;
            currentGrabber = null;
            foreach (var changedNetworkedVarName in changeDetector.DetectChanges(this, out var previous, out var current))
            {
                if (changedNetworkedVarName == nameof(CurrentGrabber))
                {
                    var grabberReader = GetBehaviourReader<NetworkHandColliderGrabber>(changedNetworkedVarName);
                    previousGrabber = grabberReader.Read(previous);
                    currentGrabber = grabberReader.Read(current);
                    return true;
                }
            }
            return false;
        }

        public override void FixedUpdateNetwork()
        {
            // Check if the grabber changed
            if (TryDetectGrabberChange(funChangeDetector, out var previousGrabber, out var currentGrabber))
            {
                if (previousGrabber)
                {
                    // Object ungrabbed
                    UnlockObjectPhysics();
                }
                if (currentGrabber)
                {
                    // Object grabbed
                    LockObjectPhysics();
                }
            }

            // We only update the object position if we have the state authority
            if (!Object.HasStateAuthority) return;

            if (!IsGrabbed) return;
            // Follow grabber, adding position/rotation offsets
            Follow(followedTransform: CurrentGrabber.transform, LocalPositionOffset, LocalRotationOffset);
        }

        private void Update()
        {
            if (Runner)
            {
                // Velocity tracking
                lastMoves[lastMoveIndex] = transform.position - lastPosition;
                lastAngularVelocities[lastMoveIndex] = previousRotation.AngularVelocityChange(transform.rotation, Time.deltaTime);
                lastDeltaTime[lastMoveIndex] = Time.deltaTime;
                lastMoveIndex = (lastMoveIndex + 1) % 5;
                lastPosition = transform.position;
                previousRotation = transform.rotation;
            }
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
            Follow(followedTransform: CurrentGrabber.hand.transform, LocalPositionOffset, LocalRotationOffset);
        }

        void ExtrapolateWhileTakingAuthority()
        {
            // No need to extrapolate if the object is not really grabbed
            if (grabberWhileTakingAuthority == null) return;

            // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
            // We use grabberWhileTakingAuthority instead of CurrentGrabber as we are currently waiting for the authority transfer: the network vars are not already set, so we use the temporary versions
            Follow(followedTransform: grabberWhileTakingAuthority.hand.transform, localPositionOffsetWhileTakingAuthority, localRotationOffsetWhileTakingAuthority);
        }

        void Follow(Transform followedTransform, Vector3 localPositionOffsetToFollowed, Quaternion localRotationOffsetTofollowed)
        {
            transform.position = followedTransform.TransformPoint(localPositionOffsetToFollowed);
            transform.rotation = followedTransform.rotation * localRotationOffsetTofollowed;
        }
    }
}
