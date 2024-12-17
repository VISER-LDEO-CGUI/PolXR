using UnityEngine;

namespace Fusion.Addons.Physics
{
  public abstract partial class NetworkRigidbodyBase {
    /// <summary>
    /// The rigidbody position
    /// </summary>
    public abstract Vector3    RBPosition    { get; set; }
    /// <summary>
    /// The rigidbody rotation
    /// </summary>
    public abstract Quaternion RBRotation    { get; set; }
    /// <summary>
    /// Signals if the rigidbody is kinematic
    /// </summary>
    public abstract bool       RBIsKinematic { get; set; }
  }

  public abstract partial class NetworkRigidbody<RBType, PhysicsSimType> : IBeforeAllTicks, IAfterTick {

    // PhysX/Box2D abstractions
    
    protected abstract void ApplyRBPositionRotation(RBType rb, Vector3 pos, Quaternion rot);
    
    protected abstract void CaptureRBPositionRotation(RBType rb, ref NetworkRBData data);
    protected abstract void CaptureExtras(RBType             rb, ref NetworkRBData data);
    protected abstract void ApplyExtras(RBType               rb, ref NetworkRBData data);

    protected abstract NetworkRigidbodyFlags GetRBFlags(RBType rb);
    
    protected abstract bool GetRBIsKinematic(RBType rb);
    protected abstract void SetRBIsKinematic(RBType rb, bool kinematic);
    protected abstract int  GetRBConstraints(RBType rb);
    protected abstract void SetRBConstraints(RBType rb, int constraints);
    
    protected abstract bool IsRBSleeping(RBType rb);
    protected abstract void ForceRBSleep(RBType rb);
    protected abstract void ForceRBWake( RBType rb);
    
    // Main NRB logic

    private int _remainingSimulationsCount;
    
    void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount) {

      // Capture the number of ticks about to be simulated.
      // We use this in AfterTick() to limit capturing state to only the last two ticks.
      _remainingSimulationsCount = tickCount;
      
      // Recenter the interpolation target. TODO: Can get more selective/efficient with this later
      if (_targIsDirtyFromInterpolation && _interpolationTarget) {
        _interpolationTarget.localPosition = default;
        _interpolationTarget.localRotation = Quaternion.identity;
        if (SyncScale) {
          _interpolationTarget.localScale = new Vector3(1f, 1f, 1f);
        }
      }

      // A dirty root should always reset at the start of the simulation loop (for both state authority and predicted).
      // Predicted objects should always reset at the start of re-simulation - in all cases.
      if (_rootIsDirtyFromInterpolation || (_clientPrediction && resimulation)) {
        CopyToEngine(resimulation);
      }
    }

    // Simulation results for State Authority outgoing updates, and for interpolation.
    public void AfterTick() {

      // Never capture more than the last two ticks of a complete simulation loop
      // Interpolation will only ever need the last two.
      // StateAuthority will only need the last one fully captured.
      int remainingTicks = _remainingSimulationsCount--;
      if (remainingTicks > 2) {
        return;
      }
      
      // State Authority capture the last two Forward ticks. Only the last forward tick needs a full capture for networking.
      if (HasStateAuthority) { 
        CopyToBuffer(remainingTicks == 2);
      } else {
        // Non-StateAuth clients only need capture values for interpolation if they are InSimulation (predicted)
        if (Object.IsInSimulation) {
          CopyToBuffer(true);
        }
      }
    }

    /// <summary>
    /// Copies the state of the Rigidbody to the Fusion state.
    /// </summary>
    /// <param name="captureTRSPOnly">If the captured data is only used for interpolation (not for networking), then skip capturing extras (vel/angVel/tensor)</param>
    protected virtual void CopyToBuffer(bool captureTRSPOnly) {
      
      var tr            = _transform;
      var rb            = _rigidbody;
      var flags         = GetRBFlags(rb);
      var syncParent    = SyncParent;
      var useWorldSpace = !syncParent;

      // Capture Parenting and handle auto AOI override
      if (syncParent) {
        // Debug.LogWarning($"<color=yellow>Copy From Engine {Object.StateAuthority}</color>");

        // Parenting handling only applies to the MainTRSP (the NO root NetworkTRSP).
        if (IsMainTRSP) {
          // If Sync Parent is enabled, set any nested parent NetworkObject as the AreaOfInterestOverride.
          // Player Interest in this object will always be determined by player interest in the current parent NO.
          var parent = tr.parent;

          // If no parent transform is present, this is simple.
          if (parent == null) {
            State.AreaOfInterestOverride = default;
            Data.TRSPData.Parent         = default;
          } 
          // If there is a parent transform, we need to determine if it is a valid NB or a non-networked transform.
          else {
        
            if (parent.TryGetComponent<NetworkBehaviour>(out var parentNB)) {
              if (_aoiEnabled) {
                SetAreaOfInterestOverride(parentNB.Object);
              } 
              Data.TRSPData.Parent = parentNB;
            } else {
              State.AreaOfInterestOverride = default;
              Data.TRSPData.Parent  = NetworkTRSPData.NonNetworkedParent;
              useWorldSpace = true;
            }
          }
        } else {
          // Reset to default in case SyncParent was enabled/disabled at runtime
          State.AreaOfInterestOverride = default;
        }
      } 
      
      // Capture RB State
      if ((flags & NetworkRigidbodyFlags.IsKinematic) != 0) {
        Vector3    position;
        Quaternion rotation;
        if (useWorldSpace) {
          // Unknown parenting requires world space for AOI to behave
          position = tr.position;
          rotation = tr.rotation;
        } else {
          position = tr.localPosition;
          rotation = tr.localRotation;
        }

        Data.TRSPData.Position = position;
        // For full precision rotation, we store the rotation outside of the TRSP struct (which compresses rotation to 32 bits)
        if (UsePreciseRotation) {
          Data.FullPrecisionRotation = rotation;
        } else {
          Data.TRSPData.Rotation = rotation;
        }

      } else {
        CaptureRBPositionRotation(rb, ref Data);
        
        // We don't need to store/network any physics info if there is no client prediction...
        // UNLESS we need to rewind the root transform because it is being used for interpolation.
        // then it is needed for the StateAuthority in OnBeforeAllTicks after Render.
        if (!captureTRSPOnly && (_clientPrediction || (ReferenceEquals(_interpolationTarget, null) && Object.HasStateAuthority))) {
          CaptureExtras(rb, ref Data);
        }
      }

      if (SyncScale) {
        Data.TRSPData.Scale = tr.localScale;
      }

      // When sleeping, the uncompressed value is used.
      if ((flags & NetworkRigidbodyFlags.IsSleeping) != 0) {
        if (useWorldSpace) {
          Data.FullPrecisionPosition =  tr.position;
          Data.FullPrecisionRotation =  tr.rotation;
        } else {
          Data.FullPrecisionPosition =  tr.localPosition;
          Data.FullPrecisionRotation =  tr.localRotation;
        }
      }
      Data.Flags = (flags, GetRBConstraints(rb));
    }
    
    /// <summary>
    /// Copies the Fusion snapshot state onto the Rigidbody.
    /// </summary>
    /// <param name="predictionReset">Indicates if this is a reset from a remote server state, in which case everything needs to be reverted to state
    /// Otherwise, if it is for the State Authority or a non-simulated proxy - only the TRSP needs to be reset from interpolation changes. (not velocity etc)</param>
    protected virtual void CopyToEngine(bool predictionReset) {
      
      var (flags, constraints) = Data.Flags;
      var  tr = _transform;
      var  rb = _rigidbody;
      bool isParented;
      var  syncParent    = SyncParent;
      var  useWorldSpace = !syncParent;

      Vector3    pos;
      Quaternion rot;

      // For non-kinematic states, test for sleep conditions - otherwise just push the local state right to the transform for kinematic.
      if (syncParent) {
        // Important to know if we have a non-networked parent, as TRS values will be in world space
        bool hasNonNetworkedParent = Data.TRSPData.Parent == NetworkTRSPData.NonNetworkedParent;
        
        var  currentParent         = tr.parent;
        if (Data.TRSPData.Parent != default) {
          isParented = true;

          if (hasNonNetworkedParent) {
            useWorldSpace = true;
            // The networked parent indicates that the parent does not have a NB and therefore is unknowable...
            // Do nothing in regards to parenting for this case - we are trusting the dev to know what they are doing.
          }
          else if (Runner.TryFindBehaviour(Data.TRSPData.Parent, out var found)) {
            var foundTransform = found.transform;
            if (ReferenceEquals(foundTransform, currentParent) == false) {
              tr.SetParent(foundTransform);
              var it = _interpolationTarget;
              if (ReferenceEquals(it, null) == false) {
                it.localPosition = default;
                it.localRotation = Quaternion.identity;
              }
            }
          } else {
            Debug.LogError($"Cannot find NetworkBehaviour.");
            OnParentNotFound();
          }

        } else {
          isParented = false;

          // TRSPData indicates no parenting
          if (currentParent) {
            tr.SetParent(null);
          }
        }
      } else {
        isParented = false;
      }
      
      var networkedIsSleeping  = (flags & NetworkRigidbodyFlags.IsSleeping)  != 0;
      var networkedIsKinematic = (flags & NetworkRigidbodyFlags.IsKinematic) != 0;
      var currentIsSleeping    = IsRBSleeping(rb);
      
      // If the State Authority is asleep, it will have valid uncompressed pos/rot values.
      if (networkedIsSleeping) {
        pos = Data.FullPrecisionPosition;
        rot = Data.FullPrecisionRotation;
      } else {
        pos = Data.TRSPData.Position;
        rot = UsePreciseRotation ? Data.FullPrecisionRotation : Data.TRSPData.Rotation;
      }
            
      // Both local and networked state are sleeping and in agreement - avoid waking the RB locally.
      // This test of position and rotation can possibly be removed by developers without consequence for many use cases.
      bool avoidWaking = !_rootIsDirtyFromInterpolation && currentIsSleeping && networkedIsSleeping && tr.localPosition == pos && tr.localRotation == rot;
      
      if (networkedIsKinematic != GetRBIsKinematic(rb)) {
        SetRBIsKinematic(rb, networkedIsKinematic);;
      }

      // Apply position and rotation
      if (avoidWaking == false) {

        // Push state to transform always if parented, and if the transform was dirtied by interpolation
        if (_rootIsDirtyFromInterpolation || !syncParent || isParented) {
          if (useWorldSpace) {
            tr.SetPositionAndRotation(pos, rot);
          } else {
            tr.localPosition = pos;
            tr.localRotation = rot;
          }
        }
        // Only push TRS data if it is valid world space (root or non-networked parent)
        // otherwise values are local space so this would be invalid.
        if (isParented == false || useWorldSpace) {
          ApplyRBPositionRotation(rb, pos, rot);
        }
        _rootIsDirtyFromInterpolation = false;
      }

      if (SyncScale) {
        tr.localScale = Data.TRSPData.Scale;
      }
      
      // Only apply extras and test for sleep handling for prediction resimulations
      // Not when just undoing interpolation TRSP changes.
      if (predictionReset && networkedIsKinematic == false) {

        ApplyExtras(rb, ref Data);
        SetRBConstraints(rb, constraints);
      
        // Local state is already in agreement with network, can skip sleep handling
        if (avoidWaking) {
          return;
        }
        
        // If sleeping states disagree, we need to intervene.
        if (currentIsSleeping != networkedIsSleeping) {
          if (networkedIsSleeping == false) {
            ForceRBWake(rb);
          } else if (IsRigidbodyBelowSleepingThresholds(rb)) {
            // Devs may want to comment this out, if their physics sim experiences hitching when waking objects with collisions.
            // This is here to make resting states 100% accurate, but ForceSleep can cause a hitch in re-simulations under very
            // specific conditions.
            ForceRBSleep(rb);
          }          
        }
      }
    }
  }
}
