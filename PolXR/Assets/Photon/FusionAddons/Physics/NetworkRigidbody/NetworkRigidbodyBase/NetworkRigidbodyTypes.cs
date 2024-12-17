using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion.Addons.Physics {
  
  [StructLayout(LayoutKind.Explicit)]
  public struct NetworkRBData : INetworkStruct {
    
    public const int WORDS = NetworkTRSPData.WORDS + 24;
    public const int SIZE  = WORDS * Allocator.REPLICATE_WORD_SIZE;
    
    [FieldOffset(0)]
    public NetworkTRSPData TRSPData;

    [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
    public FloatCompressed Drag;
    
    [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
    public FloatCompressed AngularDrag;
    
    [FieldOffset((NetworkTRSPData.WORDS + 2) * Allocator.REPLICATE_WORD_SIZE)]
    public FloatCompressed Mass;
    
    [FieldOffset((NetworkTRSPData.WORDS + 3) * Allocator.REPLICATE_WORD_SIZE)]
    int _flags;

    public (NetworkRigidbodyFlags flags, int constraints) Flags {
      get {
        var f = (NetworkRigidbodyFlags)((_flags) & 0xFF);
        var c = (int)((_flags >> 8) & 0xFF);
        return (f, c);
      }
      set {
        var (f, c) = value;
        _flags =  (int)f;
        _flags |= (int)c << 8;
      }
    }
    
    // 3D
    [FieldOffset((NetworkTRSPData.WORDS + 4) * Allocator.REPLICATE_WORD_SIZE)]
    public Vector3Compressed LinearVelocity;
    
    [FieldOffset((NetworkTRSPData.WORDS + 7) * Allocator.REPLICATE_WORD_SIZE)]
    public Vector3Compressed AngularVelocity;

    public Vector2 LinearVelocity2D {
      get => LinearVelocity;
      set => LinearVelocity = value;
    }
    
    public float AngularVelocity2D {
      get => AngularVelocity.Z;
      set => AngularVelocity.Z = value;
    }
    
    // 2D
    // Use the Z axis of Velocity to store 2D gravity Scale
    public float GravityScale2D {
      get => LinearVelocity.Z;
      set => LinearVelocity.Z = value;
    }
    
    // Sleep states
    
    [FieldOffset((NetworkTRSPData.WORDS + 10) * Allocator.REPLICATE_WORD_SIZE)]
    public Vector3 FullPrecisionPosition;
    
    [FieldOffset((NetworkTRSPData.WORDS + 13) * Allocator.REPLICATE_WORD_SIZE)]
    public Quaternion FullPrecisionRotation;
    
    // Smooth teleport handling - these values are used as the interpolation To value leading up to a teleport.
    [FieldOffset((NetworkTRSPData.WORDS + 17) * Allocator.REPLICATE_WORD_SIZE)]
    public Vector3Compressed TeleportPosition;
    
    [FieldOffset((NetworkTRSPData.WORDS + 20) * Allocator.REPLICATE_WORD_SIZE)]
    public QuaternionCompressed TeleportRotation;
    
  }
  
  /// <summary>
  /// Networked flags representing a 2D or 3D rigid body state and characteristics.
  /// </summary>
  [Flags]
  public enum NetworkRigidbodyFlags : byte {
    /// <summary>
    /// Networked kinematic state.
    /// See also <see cref="Rigidbody.isKinematic"/> or <see cref="Rigidbody2D.isKinematic"/>.
    /// </summary>
    IsKinematic = 1 << 0,

    /// <summary>
    /// Networked sleeping state.
    /// See also <see cref="Rigidbody.IsSleeping"/> or <see cref="Rigidbody2D.IsSleeping"/>.
    /// </summary>
    IsSleeping = 1 << 1,
  
    /// <summary>
    /// Networked <see cref="Rigidbody.useGravity"/> state. Not used with 2D rigid bodies.
    /// </summary>
    UseGravity = 1 << 2,
  }
  
  [Serializable]
  public struct TRSThresholds {
    /// <summary>
    /// If enabled, the energy value of the networked state will be used to determine if interpolation will be applied.
    /// Only applicable when there is no Interpolation Target set.
    /// </summary>
    [InlineHelp]
    public bool UseEnergy;
    /// <summary>
    /// The Magnitude of the difference between the current position of the Rigidbody and interpolated position.
    /// If the Magnitude of the difference is less than this value, then the Rigidbody will not be changed during Render,
    /// allowing the Rigidbody to sleep, thus retaining cached friction states.
    /// A value of 0 indicates that this threshold should not be factored in to determining if interpolation occurs.
    /// </summary>
    [InlineHelp]
    [Unit(Units.Units)]
    public float Position;
    /// <summary>
    /// The minimum Quanternion.Angle difference between the current rotation angle of the Rigidbody and interpolated rotation, for interpolation to be applied.
    /// If the angle between current and interpolated values are less than this, then the transform will not be moved for interpolation,
    /// allowing the Rigidbody to sleep, thus retaining cached friction states.
    /// A value of 0 indicates that this threshold should not be factored in to determining if interpolation occurs.
    /// </summary>
    [InlineHelp]
    [Unit(Units.Degrees)]
    public float Rotation;
    /// <summary>
    /// The Magnitude of the difference between the current localScale of the Rigidbody and interpolated localScale.
    /// If the Magnitude of the difference is less than this value, then the Rigidbody will not be changed during Render,
    /// allowing the Rigidbody to sleep, thus retaining cached friction states.
    /// A value of 0 indicates that this threshold should not be factored in to determining if interpolation occurs.
    /// </summary>
    [InlineHelp]
    [Unit(Units.NormalizedPercentage)]
    public float Scale;
    
    public static TRSThresholds Default =>new TRSThresholds() {
      UseEnergy = true,
      Position = 0.01f,
      Rotation = 0.01f,
      Scale    = 0.01f
    };
  }
}
