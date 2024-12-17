using System;
using UnityEngine;

namespace Fusion.Addons.Physics
{
  public partial class NetworkRigidbody<RBType, PhysicsSimType> {
    
    private (Vector3? position, Quaternion? rotation, bool moving) _deferredTeleport;
    
    /// <summary>
    /// Initiate a moving teleport. This method must be in FixedUpdateNetwork() called before
    /// <see cref="RunnerSimulatePhysics3D"/> and <see cref="RunnerSimulatePhysics2D"/> have simulated physics.
    /// This teleport is deferred until after physics has simulated, and captures position and rotation values both before and after simulation.
    /// This allows interpolation leading up to the teleport to have a valid pre-teleport TO target.
    /// This is an alternative to the basic Teleport(), which causes interpolation to freeze for one tick.
    /// </summary>
    public override void Teleport(Vector3? position = null, Quaternion? rotation = null) {
      if (Object.IsInSimulation == false) {
        return;
      }
      
      _deferredTeleport = (position, rotation, true);
      // for moving, be sure to apply AFTER simulation runs, we need to capture the sim results before teleporting.
      if (_physicsSimulator.HasSimulatedThisTick) {
        ApplyDeferredTeleport();        
      } else {
        _physicsSimulator.QueueAfterSimulationCallback(ApplyDeferredTeleport);
      }
    }

    /// <summary>
    /// Called after Physics has simulated, and is where the resulting simulated RB state is captured for the teleport.
    /// </summary>
    protected virtual void ApplyDeferredTeleport() {
      bool moving = _deferredTeleport.moving;
      
      if (moving) {
        // For moving teleports this is happening after Physics.Simulate
        // So we can capture the results of the simulation before applying the teleport.
        Data.TeleportPosition = _transform.position;
        Data.TeleportRotation = _transform.rotation;
      } 

      if (_deferredTeleport.position.HasValue) {
        _transform.position    = _deferredTeleport.position.Value;
        RBPosition             = _deferredTeleport.position.Value;
        Data.TRSPData.Position = _deferredTeleport.position.Value;
      }
      if (_deferredTeleport.rotation.HasValue) {
        _transform.rotation    = _deferredTeleport.rotation.Value;
        RBRotation             = _deferredTeleport.rotation.Value;
        
        if (UsePreciseRotation) {
          Data.FullPrecisionRotation = _deferredTeleport.rotation.Value;
        } else {
          Data.TRSPData.Rotation = _deferredTeleport.rotation.Value;
        }
      }
      IncrementTeleportKey(moving);
    }
    
    protected virtual void IncrementTeleportKey(bool moving) {
      // Keeping the key well under 1 byte in size 
      var key = Math.Abs(Data.TRSPData.TeleportKey) + 1;
      if (key > 30) {
        key = 1;
      }
      // Positive indicates non-moving teleport, Negative indicates moving teleport
      Data.TRSPData.TeleportKey = moving ? -key : key;
    }

  }
}