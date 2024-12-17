
using UnityEngine;

namespace Fusion.Addons.Physics {
  using Physics = UnityEngine.Physics;

  [DisallowMultipleComponent]
  [RequireComponent(typeof(Rigidbody))]
  [NetworkBehaviourWeaved(NetworkRBData.WORDS)]
  public class NetworkRigidbody3D : NetworkRigidbody<Rigidbody, RunnerSimulatePhysics3D> {
    
    public override Vector3 RBPosition {
      get => _rigidbody.position;
      set => _rigidbody.position = value;
    }
    public override Quaternion RBRotation {
      get =>_rigidbody.rotation;
      set => _rigidbody.rotation = value;
    }
    public override bool RBIsKinematic {
      get => _rigidbody.isKinematic;
      set => _rigidbody.isKinematic = value;
    }

    protected override void Awake() {
      base.Awake();
#if UNITY_2022_3_OR_NEWER
      AutoSimulateIsEnabled = Physics.simulationMode != SimulationMode.Script;
#else
      AutoSimulateIsEnabled = Physics.autoSimulation;
#endif
    }

    protected override bool GetRBIsKinematic(Rigidbody rb) {
      return rb.isKinematic;
    }    
    protected override void SetRBIsKinematic(Rigidbody rb, bool kinematic) {
      if (rb.isKinematic != kinematic) {
        rb.isKinematic = kinematic;
      }
    }
    
    protected override void CaptureRBPositionRotation(Rigidbody rb, ref NetworkRBData data) {
      data.TRSPData.Position = rb.position;
      if (UsePreciseRotation) {
        data.FullPrecisionRotation = rb.rotation;
      } else {
        data.TRSPData.Rotation = rb.rotation;
      }
    }
    protected override void ApplyRBPositionRotation(Rigidbody rb, Vector3 pos, Quaternion rot) {
      rb.position = pos;
      rb.rotation = rot;     
    }

    protected override NetworkRigidbodyFlags GetRBFlags(Rigidbody rb) {
      var flags = default(NetworkRigidbodyFlags);
      if (rb.isKinematic)  { flags |= NetworkRigidbodyFlags.IsKinematic; }
      if (rb.IsSleeping()) { flags |= NetworkRigidbodyFlags.IsSleeping; }
      if (rb.useGravity)   { flags |= NetworkRigidbodyFlags.UseGravity; }
      return flags;
    }
    protected override int GetRBConstraints(Rigidbody rb) {
      return (int)rb.constraints;
    }
    protected override void SetRBConstraints(Rigidbody rb, int constraints) {
      rb.constraints = (RigidbodyConstraints)constraints;
    }
    
    protected override void CaptureExtras(Rigidbody rb, ref NetworkRBData data) {
      data.Mass        = rb.mass;
      data.Drag        = rb.drag;
      data.AngularDrag = rb.angularDrag;

      data.LinearVelocity  = rb.velocity;
      data.AngularVelocity = rb.angularVelocity;
    }
    protected override void ApplyExtras(Rigidbody rb, ref NetworkRBData data) {
      rb.mass        = data.Mass;
      rb.drag        = data.Drag;
      rb.angularDrag = data.AngularDrag;
      
      rb.velocity        = data.LinearVelocity;
      rb.angularVelocity = data.AngularVelocity;
    }

    public override void ResetRigidbody() {
      base.ResetRigidbody();
      var rb = _rigidbody;
      rb.velocity        = default;
      rb.angularVelocity = default;
    }

    protected override bool IsRBSleeping(Rigidbody rb) => rb.IsSleeping();
    protected override void ForceRBSleep(Rigidbody rb) => rb.Sleep();
    protected override void ForceRBWake( Rigidbody rb) => rb.WakeUp();
    
    protected override bool IsRigidbodyBelowSleepingThresholds(Rigidbody rb) {
      var energy  = rb.mass * rb.velocity.sqrMagnitude;
      var angVel  = rb.angularVelocity;
      var inertia = rb.inertiaTensor;

      energy += inertia.x * (angVel.x * angVel.x);
      energy += inertia.y * (angVel.y * angVel.y);
      energy += inertia.z * (angVel.z * angVel.z);

      // Mass-normalized
      energy /= 2.0f * rb.mass;

      return energy <= Physics.sleepThreshold;
    }

    protected override bool IsStateBelowSleepingThresholds(NetworkRBData data) {
      var energy  = data.Mass * ((Vector3)data.LinearVelocity).sqrMagnitude;
      var angVel  = ((Vector3)data.AngularVelocity);
      var inertia = _rigidbody.inertiaTensor;

      energy += inertia.x * (angVel.x * angVel.x);
      energy += inertia.y * (angVel.y * angVel.y);
      energy += inertia.z * (angVel.z * angVel.z);

      // Mass-normalized
      energy /= 2.0f * data.Mass;

      return energy <= Physics.sleepThreshold;
    }
  }
}