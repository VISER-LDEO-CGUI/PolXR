using UnityEngine;

namespace Fusion.Addons.Physics {
  
  /// <summary>
  /// Base class for NRB which contains no physics references.
  /// </summary>
  public abstract partial class NetworkRigidbodyBase : NetworkTRSP, INetworkTRSPTeleport {
    protected new ref NetworkRBData Data => ref ReinterpretState<NetworkRBData>();

    /// <summary>
    /// Enables synchronization of Scale. Keep this disabled if you are not altering the scale of this transform, to reduce CPU utilization.
    /// </summary>
    [InlineHelp]
    [SerializeField]
    public bool SyncScale;
    
    /// <summary>
    /// Enables synchronization of Parent. Keep this disabled if you are not altering the parent of this transform, to reduce CPU utilization.
    /// </summary>
    [InlineHelp]
    [SerializeField]
    public bool SyncParent = true;
    
    /// <summary>
    /// Designate a render-only (non-physics) target Transform for all interpolation.
    /// </summary>
    [InlineHelp]
    [SerializeField]
    protected Transform _interpolationTarget;
    
    /// <summary>
    /// When disabled, rotation is stored in the <see cref="NetworkTRSPData"/> rotation field, which compresses rotation to 32 bits using 'Smallest Three'.
    /// When enabled, this <see cref="NetworkTRSPData"/> rotation field is not used.
    /// Instead, rotation only uses a separate uncompressed Quaternion field which otherwise is only used to
    /// store resting values when the RB goes to sleep.
    /// </summary>
    [InlineHelp]
    [SerializeField]
    public bool UsePreciseRotation;
    
    /// <summary>
    /// Enable checks which prevent interpolation from moving the root transform during interpolation unless needed.
    /// This mitigates the issue of Physics being broken by interpolating RBs by moving the Rigidbody's transform.
    /// Only applicable if no <see cref="_interpolationTarget"/> is designated.
    /// </summary>
    [InlineHelp]
    [Space]
    [SerializeField]
    [DrawIf(nameof(_interpolationTarget), false, CompareOperator.IsZero, DrawIfMode.Hide)]
    public bool UseRenderSleepThresholds = true;

    /// <summary>
    /// Render Sleep Threshold settings.
    /// </summary>
    [InlineHelp] [SerializeField] [DrawIf(nameof(_showSleepOptions), true, mode: DrawIfMode.Hide)]
    public TRSThresholds RenderThresholds = TRSThresholds.Default;

    // used by DrawIf attribute for inspector
    protected bool _showSleepOptions => !_interpolationTarget && UseRenderSleepThresholds;

    // Cached
    protected Transform _transform;
    protected bool      _doNotInterpolate;
    protected bool      _clientPrediction;
    protected bool      _rootIsDirtyFromInterpolation;
    protected bool      _targIsDirtyFromInterpolation;
    protected bool      _aoiEnabled;
    protected bool      AutoSimulateIsEnabled { get; set; }

    public Transform InterpolationTarget {
      get => _interpolationTarget;
      set {
        if (value == null || value == transform) {
          _interpolationTarget    = null;
        } else {
#if UNITY_EDITOR
          var c = value.GetComponentInChildren<Collider>();
          if (c && c.enabled) {
            Debug.LogWarning($"Assigned Interpolation Target '{value.name}' on GameObject '{name}' contains a non-trigger collider, this may not be intended as interpolation may break physics caching, and prevent the Rigidbody from sleeping");
          }
#endif
          _interpolationTarget    = value;
        }
      }
    }
    
    /// <summary>
    /// Change the Transform (typically a child of the Rigidbody root transform) which will be moved in interpolation.
    /// When set to null, the Rigidbody Transform will be used.
    /// </summary>
    public void SetInterpolationTarget(Transform target) {
      if (target == null || target == transform) {
        _interpolationTarget    = null;
      } else {
#if UNITY_EDITOR
        var c = target.GetComponentInChildren<Collider>();
        if (c && c.enabled) {
          Debug.LogWarning($"Assigned Interpolation Target '{target.name}' on GameObject '{name}' contains a non-trigger collider, this may not be intended as interpolation may break physics caching, and prevent the Rigidbody from sleeping");
        }
#endif
        _interpolationTarget    = target;
      }
    }

    protected virtual void OnValidate() {
      SetInterpolationTarget(_interpolationTarget);
    }
    
    public override void Spawned() {
      _aoiEnabled = Runner.Config.Simulation.AreaOfInterestEnabled;

      // Don't interpolate on Dedicated Server
      _doNotInterpolate = Runner.Mode == SimulationModes.Server;
      
      // Validate the serialized target.
      SetInterpolationTarget(_interpolationTarget);

      RBPosition = transform.position;
      RBRotation = transform.rotation;
    }
    
    public abstract void Teleport(Vector3? position = null, Quaternion? rotation = null);
  }

}
