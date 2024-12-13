using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Detect and synchronize XR device position with gamobject transform
/// 
/// Note: compatible with Unity 2020 and more. For Unity 2019, InputDeviceRole should be used instead of InputDeviceCharacteristics
/// </summary>
namespace Fusion.XR.Shared.Rig
{
    public class XRControllerInputDevice : XRInputDevice
    {
        public enum ControllerSide
        {
            Left,
            Right
        }

        [Header("Hand type")]
        public ControllerSide side = ControllerSide.Right;

        protected override InputDeviceCharacteristics DesiredCharacteristics => InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | (side == ControllerSide.Left ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right);
    }
}
