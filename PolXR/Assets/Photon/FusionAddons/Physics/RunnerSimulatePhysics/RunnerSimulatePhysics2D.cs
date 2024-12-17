using System;
using UnityEngine;

namespace Fusion.Addons.Physics {
  /// <summary>
  /// Fusion component for handling Physics2D.Simulate(). 
  /// </summary>
  [DisallowMultipleComponent]
  public class RunnerSimulatePhysics2D : RunnerSimulatePhysicsBase<PhysicsScene2D> {
    
    protected override void OverrideAutoSimulate(bool set) {
      _physicsAutoSimRestore = (PhysicsTimings)Physics2D.simulationMode;
      if (set) {
        Physics2D.simulationMode = (SimulationMode2D)_physicsTiming;
      } else {
        Physics2D.simulationMode = SimulationMode2D.Script;
      }
    }

    protected override void RestoreAutoSimulate() {
      Physics2D.simulationMode = (SimulationMode2D)_physicsAutoSimRestore;
    }

    protected override bool AutoSyncTransforms {
      get => Physics2D.autoSyncTransforms;
      set => Physics2D.autoSyncTransforms = value;
    }

    protected override PhysicsTimings UnityPhysicsPhysicsMode => (PhysicsTimings)Physics2D.simulationMode;

    protected override void SimulatePrimaryScene(float deltaTime) {
      if (Runner.SceneManager.TryGetPhysicsScene2D(out var physicsScene)) {
        if (physicsScene.IsValid()) {
          physicsScene.Simulate(deltaTime);
        } else {
          Physics2D.Simulate(deltaTime);
        }
      }
    }

    protected override void SimulateAdditionalScenes(float deltaTime, bool isForward) {
      if (_additionalScenes == null || _additionalScenes.Count == 0) {
        return;
      }
      var defaultPhysicsScene = Physics2D.defaultPhysicsScene;
      foreach (var scene in _additionalScenes) {
        if (!scene.ForwardOnly || isForward) {
          if (scene.PhysicsScene != defaultPhysicsScene || Physics2D.simulationMode == SimulationMode2D.Script) {
            scene.PhysicsScene.Simulate(deltaTime);
          }
        }
      }
    }
  }
}
