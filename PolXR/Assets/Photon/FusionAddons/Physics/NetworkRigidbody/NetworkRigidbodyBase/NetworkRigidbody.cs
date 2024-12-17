using UnityEngine;

namespace Fusion.Addons.Physics {
  using Physics = UnityEngine.Physics;

  public abstract partial class NetworkRigidbody<RBType, PhysicsSimType> : NetworkRigidbodyBase, IStateAuthorityChanged, ISimulationExit
    where RBType          : Component
    where PhysicsSimType  : RunnerSimulatePhysicsBase {
    
    /// <summary>
    /// Rigidbody component
    /// </summary>
    public RBType Rigidbody => _rigidbody;
    
    // Cached
    protected RBType         _rigidbody;
    protected PhysicsSimType _physicsSimulator;
    protected bool           _originalIsKinematic;
    
    protected virtual void Awake() {
      TryGetComponent(out _transform);
      TryGetComponent(out _rigidbody);
      
      // Store the original state. Used in Despawn to reset for pooling.
      _originalIsKinematic = RBIsKinematic;
    }

    void ISimulationExit.SimulationExit() {
      // RBs removed from simulation will stop getting Copy calls, and will only be running Render
      // So we need to set them as kinematic here (to avoid relentless checks in Render)
      SetRBIsKinematic(_rigidbody, true);
    }

    public override void Spawned() {
      base.Spawned();

      // Force Proxies to be kinematic.
      // This can be removed if you specifically instruct proxies to simulate with Runner.SetIsSimulated()
      // and want proxies to predict (not applicable to Shared Mode).
      if (IsProxy) {
        SetRBIsKinematic(_rigidbody, true);
      }
      
      EnsureHasRunnerSimulatePhysics();
      _clientPrediction = Runner.Topology != Topologies.Shared && !_physicsSimulator.ForwardOnly;

      if (HasStateAuthority) {
        CopyToBuffer(false);
      } else {
        // Mark the root as dirty to force CopyToEngine to update the transform.
        _rootIsDirtyFromInterpolation = true;
        CopyToEngine(true);
        // This has to be here after CopyToEngine, or it will set Kinematic right back.
        if (Object.IsInSimulation == false) {
          SetRBIsKinematic(_rigidbody, true);
        }
      }
    }


    public override void Despawned(NetworkRunner runner, bool hasState) {
      base.Despawned(runner, hasState);
      ResetRigidbody();
    }

    /// <summary>
    /// Reset velocity and other values to defaults, so that pooled objects do not Spawn()
    /// with previous velocities, etc.
    /// </summary>
    public virtual void ResetRigidbody() {
      SetRBIsKinematic(_rigidbody, _originalIsKinematic);
    }

     public virtual void StateAuthorityChanged() {
      // Debug.Log($"Auth Change {Runner.LocalPlayer} {name} {HasStateAuthority} {HasInputAuthority}");
    
      if (Object.IsProxy) {
        SetRBIsKinematic(_rigidbody, true);
      } else {
        // This assumes that the initial kinematic state (the state of the prefab or scene object)
        // of the RB is what is intended. Users may want to override this with their own handling
        // if they manually set the Rigidbody to kinematic via code during or after Spawned().
        SetRBIsKinematic(_rigidbody, _originalIsKinematic);
      }
    }
    
    private void EnsureHasRunnerSimulatePhysics() {
      if (_physicsSimulator) {
        return;
      }
      
      if (Runner.TryGetComponent(out PhysicsSimType existing)) {
        _physicsSimulator = existing;
        return ;
      }

      // For Shared Mode in Single Peer mode, we by default will let Unity handle physics.
#if UNITY_2022_3_OR_NEWER
      var timing = (typeof(RBType) == typeof(Rigidbody) ? (PhysicsTimings)Physics.simulationMode : (PhysicsTimings)Physics2D.simulationMode);
#else
      var timing = (typeof(RBType) == typeof(Rigidbody) ? (PhysicsTimings)(Physics.autoSimulation ? PhysicsTimings.FixedUpdate : PhysicsTimings.Script) : (PhysicsTimings)Physics2D.simulationMode);
#endif
      
      // If all of the current mode settings allow for Unity to handled Physics(2D).Simulate() exit out
      if (Application.isPlaying                                           && 
          (bool)Runner                                                    && 
          Runner.IsRunning                                                &&
          Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Single && 
          (Runner.GameMode == GameMode.Shared)                            &&
          timing != PhysicsTimings.Script) {
        return;
      }
      
      Debug.LogWarning($"No {typeof(PhysicsSimType).Name} present on NetworkRunner, but is required by {GetType().Name} on gameObject '{name}'. Adding one using default settings.");
      _physicsSimulator = Runner.gameObject.AddComponent<PhysicsSimType>();
      Runner.AddGlobal(_physicsSimulator);
    }
    
    /// <summary>
    /// Developers can override this method to add handling for parent not existing locally.
    /// </summary>
    protected virtual void OnParentNotFound() {
      Debug.LogError($"Parent does not exist locally");
    }
  }
}
