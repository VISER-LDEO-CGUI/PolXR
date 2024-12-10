#define USE_PHYSICSADDON
#if USE_PHYSICSADDON
using Fusion.Addons.Physics;
#endif
using Fusion;
using Fusion.XR.Host.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    [System.Serializable]
    public struct DetailedGrabInfo : INetworkStruct
    {
        public PlayerRef grabbingUser;
        public NetworkBehaviourId grabberId;
        public GrabInfo grabInfo;
    }


    [RequireComponent(typeof(PhysicsGrabbable))]
    [DefaultExecutionOrder(NetworkPhysicsGrabbable.EXECUTION_ORDER)]
    public class NetworkPhysicsGrabbable : NetworkGrabbable, IInputAuthorityLost
    {
        [HideInInspector]
        public NetworkRigidbody3D networkRigidbody;

        [Networked]
        public DetailedGrabInfo DetailedGrabInfo { get; set; }

        ChangeDetector changeDetector;

        #region Feedback configuration         
        [System.Serializable]
        public struct PseudoHapticFeedbackConfiguration
        {
            public bool enablePseudoHapticFeedback;
            public float minNonContactingDissonance;
            public float minContactingDissonance;
            public float maxDissonanceDistance;
            public float vibrationDuration;
        }

        [Header("Feedback configuration")]
        public PseudoHapticFeedbackConfiguration pseudoHapticFeedbackConfiguration = new PseudoHapticFeedbackConfiguration {
            enablePseudoHapticFeedback = true,
            minNonContactingDissonance = 0.05f,
            minContactingDissonance = 0.005f,
            maxDissonanceDistance = 0.60f,
            vibrationDuration = 0.06f
        };
        #endregion

        [HideInInspector]
        public bool isPseudoHapticDisplayed = false;

        [Networked]
        NetworkBool IsColliding { get; set; }  = false;

        // PID Memorization
        [Networked]
        Vector3 PidLastError { get; set; }
        [Networked]
        Vector3 PidErrorIntegration { get; set; }

        [HideInInspector]
        public PhysicsGrabbable grabbable;
        struct Localization
        {
            public float time;
            public Vector3 pos;
            public Quaternion rot;
        }

        public bool displayInRemoteTimeFrameWhenGrabbed = true;
        List<Localization> lastLocalizations = new List<Localization>();

        public override NetworkGrabber CurrentGrabber
        {
            get
            {
                if (willReceiveInputAuthority)
                {
                    return incomingGrabber;
                }
                else if (DetailedGrabInfo.grabbingUser != PlayerRef.None)
                {
                    return GrabberForId(DetailedGrabInfo.grabberId);
                }
                return null;
            }
        }

        bool willReceiveInputAuthority = false;
        RigPart previousGrabbingSide = RigPart.None;

        // Stored to cancel the wait for input authority if someone catched the object quickly just after us
        float inputAuthorityChangeRequestTime = 0;
        // Stored to avoid anticipating a grab too early in resim
        Tick inputAuthorityChangeRequestTick;
        GrabInfo incomingGrabInfo = default;
        NetworkGrabber incomingGrabber = null;

        Dictionary<(PlayerRef player, RigPart side), NetworkGrabber> cachedGrabbers = new Dictionary<(PlayerRef player, RigPart side), NetworkGrabber>();


        private void Awake()
        {
            networkRigidbody = GetComponent<NetworkRigidbody3D>();
            grabbable = GetComponent<PhysicsGrabbable>();
        }

        #region NetworkGrabbable
        // Will be called by the host and by the grabbing user (the input authority of the NetworkGrabber) upon NetworkGrabber.GrabInfo change detection
        //  For other users, will be called by the local NetworkGrabbable.DetailedGrabInfo change detection
        public override void Grab(NetworkGrabber newGrabber, GrabInfo newGrabInfo)
        {
            if (Object.InputAuthority != newGrabber.Object.InputAuthority)
            {
                if (newGrabber.Object.InputAuthority == Runner.LocalPlayer)
                {
                    // Store data to handle the grab while the input authority transfer is pending
                    willReceiveInputAuthority = true;
                    inputAuthorityChangeRequestTime = Time.time;
                    inputAuthorityChangeRequestTick = Runner.Tick;
                    incomingGrabInfo = newGrabInfo;
                    incomingGrabber = newGrabber;
                }

                // Transfering the input authority of the cube is in fact not strickly required here (as the object is fully simulated on all clients)
                if (Object.HasStateAuthority)
                {
                    Object.AssignInputAuthority(newGrabber.Object.InputAuthority);
                }
            }
            cachedGrabbers[(newGrabber.Object.InputAuthority, newGrabber.hand.side)] = newGrabber;
        }

        public override void Ungrab(NetworkGrabber previousGrabber, GrabInfo newGrabInfo)
        {
        }
        #endregion

        #region NetworkBehaviour
        public override void Spawned()
        {
            base.Spawned();

            // When an object is grabbed by an user (non host), it is not yet input authority.
            //  But still, we already want to simulate physics as soon as possible (even before receiving input authority).
            //  Hence, the need to simulate on each client.
            //  Note that it can be replaced by extrapolation code (simulating the physics locally while waiting for input authority reception,
            //  disabling the NRB Render interpolation, and making sure the resim tick do not make the object advance too fast)
            Runner.SetIsSimulated(Object, true);

            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            if (IsGrabbed)
            {
                DidGrab();
            }
        }

        public virtual void Follow(Transform followedTransform, float elapsedTime, bool isColliding)
        {
            if (grabbable.followMode == PhysicsGrabbable.FollowMode.PID)
            {
                grabbable.pid.errorIntegration = PidErrorIntegration;
                grabbable.pid.lastError = PidLastError;
            }
            grabbable.Follow(followedTransform: followedTransform, elapsedTime: elapsedTime, isColliding: isColliding);
            if(grabbable.followMode == PhysicsGrabbable.FollowMode.PID)
            {
                PidErrorIntegration = grabbable.pid.errorIntegration;
                PidLastError = grabbable.pid.lastError;
            }
        }

        public NetworkGrabber GrabberForSideAndPlayer(PlayerRef player, RigPart side)
        {
            if (cachedGrabbers.ContainsKey((player, side))) return cachedGrabbers[(player, side)];
            return null;
        }

        public NetworkGrabber GrabberForId(NetworkBehaviourId id)
        {
            if (Runner.TryFindBehaviour<NetworkGrabber>(id, out var grabber)) return grabber;
            return null;
        }

        public override void FixedUpdateNetwork()
        {
            // ---- Handle waiting for input authority reception
            if (willReceiveInputAuthority && Object.HasInputAuthority)
            {
                // Authority received
                willReceiveInputAuthority = false;
            }
            if (willReceiveInputAuthority && (Time.time - inputAuthorityChangeRequestTime) > 1)
            {
                // Authority not received (quickly grabbed by someone else ?)
                willReceiveInputAuthority = false;
            }

            // ---- Reference previous state (up to date for host / input authority only - proxies grab info will always remain at the last confirmed value)
            bool wasGrabbed = DetailedGrabInfo.grabbingUser != PlayerRef.None;
            var previousGrabberId = DetailedGrabInfo.grabberId;

            // ---- Determine grabber/grab info for this tick
            bool isGrabbed = false;
            GrabInfo grabInfo = default;
            NetworkGrabber grabber = null;
            bool grabbingWhileNotYetInputAuthority = willReceiveInputAuthority && Runner.Tick > inputAuthorityChangeRequestTick;
            if (grabbingWhileNotYetInputAuthority)
            {
                // We are taking the input authority: we anticipate the grab before being able to read GetInput, by setting "manually" the grabber
                grabInfo = incomingGrabInfo;
                grabber = incomingGrabber;
            }
            else if (GetInput<RigInput>(out var input))
            {
                // Host or input authority: we use the input to replay the exact moment of the grab/ungrab in resims
                isGrabbed = false;
                if (input.leftGrabInfo.grabbedObjectId == Id)
                {
                    isGrabbed = true;
                    grabInfo = input.leftGrabInfo;
                    PlayerRef grabbingUser = Object.InputAuthority;
                    grabber = GrabberForSideAndPlayer(grabbingUser, RigPart.LeftController);
                    previousGrabbingSide = RigPart.LeftController;
                }
                else if (input.rightGrabInfo.grabbedObjectId == Id)
                {
                    isGrabbed = true;
                    // one-hand grabbing only in this implementation
                    grabInfo = input.rightGrabInfo;
                    PlayerRef grabbingUser = Object.InputAuthority;
                    grabber = GrabberForSideAndPlayer(grabbingUser, RigPart.RightController);
                    previousGrabbingSide = RigPart.RightController;
                }
                else if (wasGrabbed && previousGrabbingSide != RigPart.None)
                {
                    grabInfo = previousGrabbingSide == RigPart.LeftController ? input.leftGrabInfo : input.rightGrabInfo;
                }
            }
            else
            {
                // Proxy
                isGrabbed = DetailedGrabInfo.grabbingUser != PlayerRef.None;
                // one-hand grabbing only in this implementation
                grabInfo = DetailedGrabInfo.grabInfo;
                if (isGrabbed) grabber = GrabberForId(DetailedGrabInfo.grabberId);
            }

            // ---- Apply following move based on grabber/grabinfo
            if (isGrabbed)
            {
                grabbable.localPositionOffset = grabInfo.localPositionOffset;
                grabbable.localRotationOffset = grabInfo.localRotationOffset;
                Follow(followedTransform: grabber.transform, elapsedTime: Runner.DeltaTime, isColliding: IsColliding);
            }

            // ---- Store DetailedGrabInfo changes
            if (isGrabbed && (wasGrabbed == false || previousGrabberId != grabber.Id))
            {
                // New Grab
                // We do not store data as proxies, unless if we are waiting for the input authority
                if (Object.IsProxy == false || grabbingWhileNotYetInputAuthority)
                {
                    DetailedGrabInfo = new DetailedGrabInfo
                    {
                        grabbingUser = grabber.Object.InputAuthority,
                        grabberId = grabber.Id,
                        grabInfo = grabInfo,
                    };
                }
            }
            if (wasGrabbed && isGrabbed == false)
            {
                // Ungrab
                // We do not store data as proxies, unless if we are waiting for the input authority
                if (Object.IsProxy == false || grabbingWhileNotYetInputAuthority)
                {
                    DetailedGrabInfo = new DetailedGrabInfo
                    {
                        grabbingUser = PlayerRef.None,
                        grabberId = previousGrabberId,
                        grabInfo = grabInfo,
                    };
                }

                // Apply release velocity (the release timing is probably between tick, so we stored in the input the ungrab velocity to have sub-tick accuracy)
                grabbable.rb.velocity = grabInfo.ungrabVelocity;
                grabbable.rb.angularVelocity = grabInfo.ungrabAngularVelocity;
            }

            // ---- Trigger callbacks and release velocity
            // Callbacks are triggered only during forward tick to avoid triggering them several time due to resims.
            // If we are waiting for input authority, we do not check (and potentially trigger) the callbacks, as the DetailedGrabInfo will temporarily be erased by the server, and so that might trigger twice the callbacks later
            if (Runner.IsForward && grabbingWhileNotYetInputAuthority == false)
            {
                TriggerCallbacksOnForwardGrabbingChanges();
            }

            // ---- Consume the isColliding value: it will be reset in the next physics simulation (used in PID based moves)
            IsColliding = false;

            if (displayInRemoteTimeFrameWhenGrabbed && Runner.IsFirstTick && Runner.IsForward == false)
            {
                // Store the first resim ticks (latest confirmed from the host) for this simulation time, in order to compute a remote timeframe in render when grabbed by a remote hand
                lastLocalizations.Add(new Localization { time = Runner.SimulationTime, pos = transform.position, rot = transform.rotation });
                while (lastLocalizations.Count > 20)
                {
                    lastLocalizations.RemoveAt(0);
                }
            }
        }


        private void TriggerCallbacksOnForwardGrabbingChanges()
        {
            foreach (var change in changeDetector.DetectChanges(this, out NetworkBehaviourBuffer previousBuffer, out NetworkBehaviourBuffer currentBuffer))
            {
                if (change == nameof(DetailedGrabInfo))
                {
                    var reader = GetPropertyReader<DetailedGrabInfo>(nameof(DetailedGrabInfo));
                    (var previousInfo, var currentInfo) = reader.Read(previousBuffer, currentBuffer);
                    bool wasGrabbingbeforeChange = previousInfo.grabbingUser != PlayerRef.None;
                    bool isGrabbingAfterChange = currentInfo.grabbingUser != PlayerRef.None;
                    if (wasGrabbingbeforeChange == false && isGrabbingAfterChange)
                    {
                        DidGrab();
                    }
                    if (wasGrabbingbeforeChange && isGrabbingAfterChange == false)
                    {
                        // If we are the player ungrabbing, and we displayed the ghost hands, we hide them
                        var wasGrabbingLocally = Runner.LocalPlayer == previousInfo.grabbingUser;
                        var previousGrabber = GrabberForId(previousInfo.grabberId);
                        if (wasGrabbingLocally && previousGrabber != null && previousGrabber.hand.LocalHardwareHand != null && previousGrabber.hand.LocalHardwareHand.localHandRepresentation != null)
                        {
                            previousGrabber.hand.LocalHardwareHand.localHandRepresentation.DisplayMesh(false);
                        }

                        DidUngrab(GrabberForId(previousInfo.grabberId));
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            if (Object.InputAuthority != Runner.LocalPlayer)
            {
                // Allow to prevent local hardware grabbing of the same object
                grabbable.isGrabbed = IsGrabbed;
            }

            if (CurrentGrabber != null && CurrentGrabber.HasInputAuthority == false && willReceiveInputAuthority == false)
            {
                /*
                 * We want to render in the remote time frame here.
                 * 
                 * As we forced the proxy simulation with SetIsSimulated (so that the physics is run locally), 
                 * the object is always simulated, so by default then the interpolation would be done between locally simulated ticks
                 * But while applying physics is important to handle collision with other simulated object, the position is not perfectly predicted,
                 * as the hand simulated position for the remote user is not moving, as it is probably, during those states
                 * 
                 * So, while the FUN position is used for local physics computation, for the final rendering of this object, we prefer to use the remote timeframe,
                 * which will interpolate between states where the hand were properly positioned to trigger the following
                 */
                Localization from = default, to = default;
                bool fromFound = false;
                bool toFound = false;
                float targetTime = Runner.RemoteRenderTime;
                int i = 0;
                foreach(var loc in lastLocalizations)
                {
                    if(loc.time < targetTime)
                    {
                        fromFound = true;
                        from = loc;
                    }
                    else
                    {
                        to = loc;
                        toFound = true;
                        break;
                    }
                    i++;
                }
                if(fromFound && toFound)
                {
                    float remoteAlpha = (float)Maths.Clamp01((targetTime - from.time) / (to.time - from.time));

                    networkRigidbody.InterpolationTarget.transform.position = Vector3.Lerp(from.pos, to.pos, remoteAlpha);
                    networkRigidbody.InterpolationTarget.transform.rotation = Quaternion.Slerp(from.rot, to.rot, remoteAlpha);
                }

            }
            // We don't place the hand on the object while we are waiting to receive the input authority as the timeframe transitioning might lead to erroneous hand repositioning
            if (IsGrabbed && willReceiveInputAuthority == false)
            {
                var handVisual = CurrentGrabber.hand.transform;
                var grabbableVisual = networkRigidbody.InterpolationTarget.transform;

                // On remote user, we want the hand to stay glued to the object, even though the hand and the grabbed object may have various interpolation
                handVisual.rotation = grabbableVisual.rotation * Quaternion.Inverse(grabbable.localRotationOffset);
                handVisual.position = grabbableVisual.position - (handVisual.TransformPoint(grabbable.localPositionOffset) - handVisual.position);

                // Add pseudo haptic feedback if needed
                ApplyPseudoHapticFeedback();
            }
        }
        #endregion

        #region Collision handling and feedback

        private void OnCollisionStay(Collision collision)
        {
            if (Object)
            {
                IsColliding = true;
            }
        }

        // Display a "ghost" hand at the position of the real life hand when the distance between the representation (glued to the grabbed object, and driven by forces) and the IRL hand becomes too great
        //  Also apply a vibration proportionnal to this distance, so that the user can feel the dissonance between what they ask and what they can do
        void ApplyPseudoHapticFeedback()
        {
            var isLocalPlayerMostRecentGrabber = Runner.LocalPlayer == DetailedGrabInfo.grabbingUser;
            if (pseudoHapticFeedbackConfiguration.enablePseudoHapticFeedback && IsGrabbed && isLocalPlayerMostRecentGrabber)
            {
                if (CurrentGrabber.hand.LocalHardwareHand.localHandRepresentation != null)
                {
                    var handVisual = CurrentGrabber.hand.transform;
                    Vector3 dissonanceVector = handVisual.position - CurrentGrabber.hand.LocalHardwareHand.transform.position;
                    float dissonance = dissonanceVector.magnitude;
                    isPseudoHapticDisplayed = (IsColliding && dissonance > pseudoHapticFeedbackConfiguration.minContactingDissonance);
                    CurrentGrabber.hand.LocalHardwareHand.localHandRepresentation.DisplayMesh(isPseudoHapticDisplayed);
                    if (isPseudoHapticDisplayed)
                    {
                        CurrentGrabber.hand.LocalHardwareHand.SendHapticImpulse(amplitude: Mathf.Clamp01(dissonance / pseudoHapticFeedbackConfiguration.maxDissonanceDistance), duration: pseudoHapticFeedbackConfiguration.vibrationDuration);
                    }
                }
            }
        }
        #endregion

        #region IInputAuthorityLost
        public void InputAuthorityLost()
        {
            // When using Object.AssignInputAuthority, SetIsSimulated will be reset to false. as we want the object to remain simulated (see Spawned), we have to set it back
            Runner.SetIsSimulated(Object, true);
        }
        #endregion
    }
}
