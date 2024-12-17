using UnityEngine;
using Fusion.Analyzer;

namespace Fusion.Addons.Physics {
  using Physics = UnityEngine.Physics;
  
  /// <summary>
  /// Fusion component for handling Physics.Simulate(). When added to a <see cref="NetworkRunner"/> GameObject, this will automatically disable 
  /// </summary>
  [DisallowMultipleComponent]
  public class RunnerSimulatePhysics3D : RunnerSimulatePhysicsBase<PhysicsScene> {

    // Unity 2022.3 made changes to PhysX (Physics3D) which made it similar to Box2d (Physics2D)
    // This changed from a basic Physics.AutoSimulate to Simulations options for FixedUpdate, Update or Script(the equivalent of auto-simulate disabled).
#if !UNITY_2022_3_OR_NEWER
    
    protected override PhysicsTimings UnityPhysicsPhysicsMode => Physics.autoSimulation ? PhysicsTimings.FixedUpdate : PhysicsTimings.Script;
    
    protected override void OverrideAutoSimulate(bool set) {
      
      if (set && _physicsTiming == PhysicsTimings.Update) {
        Debug.LogWarning($"{GetType().Name}.{nameof(_physicsTiming)} set to {PhysicsTimings.Update}, which is not valid in Unity versions below 2022.3. Changing {_physicsAuthority} to {PhysicsAuthorities.Fusion}");
        _physicsAuthority = PhysicsAuthorities.Fusion;
        set               = false;
      }
      _physicsAutoSimRestore = Physics.autoSimulation ? PhysicsTimings.FixedUpdate : PhysicsTimings.Script;
      Physics.autoSimulation = set;
    }
    
    protected override void RestoreAutoSimulate() {
      Physics.autoSimulation = _physicsAutoSimRestore == PhysicsTimings.FixedUpdate ? true : false;
    }
#else
    
    protected override PhysicsTimings UnityPhysicsPhysicsMode => (PhysicsTimings)Physics.simulationMode;

    protected override void OverrideAutoSimulate(bool set) {
      _physicsAutoSimRestore = (PhysicsTimings)Physics.simulationMode;
      if (set) {
        Physics.simulationMode = (SimulationMode)_physicsTiming;
      } else {
        Physics.simulationMode = SimulationMode.Script;
      }
    }
    
    protected override void RestoreAutoSimulate() {
      Physics.simulationMode = (SimulationMode)_physicsAutoSimRestore;
    }
#endif
    
    [StaticField(StaticFieldResetMode.None)]
    static bool? _physicsAutoSyncRestore;

    protected override bool AutoSyncTransforms {
      get => Physics.autoSyncTransforms;
      set => Physics.autoSyncTransforms = value;
    }
    
    protected override void SimulatePrimaryScene(float deltaTime) {
      if (Runner.SceneManager.TryGetPhysicsScene3D(out var physicsScene)) {
        if (physicsScene.IsValid()) {
          physicsScene.Simulate(deltaTime);
        } else {
          Physics.Simulate(deltaTime);
        }
      }
    }

    protected override void SimulateAdditionalScenes(float deltaTime, bool isForward) {
      if (_additionalScenes == null || _additionalScenes.Count == 0) {
        return;
      }
      var defaultPhysicsScene = Physics.defaultPhysicsScene;
      foreach (var scene in _additionalScenes) {
        if (!scene.ForwardOnly || isForward) {

#if UNITY_2022_3_OR_NEWER
          if (scene.PhysicsScene != defaultPhysicsScene || Physics.simulationMode == SimulationMode.Script) {
#else
          if (scene.PhysicsScene != defaultPhysicsScene || Physics.autoSimulation == false) {
#endif
            scene.PhysicsScene.Simulate(deltaTime);
          }
        }
      }
    }
  }
}
