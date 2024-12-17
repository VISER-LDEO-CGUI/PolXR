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
    public class XRHeadsetInputDevice : XRInputDevice
    {
        [Header("Camera options")]
        public bool preventAutoXRCameraTracking = true;

        private void Awake()
        {
            if (preventAutoXRCameraTracking && TryGetComponent<Camera>(out var camera))
            {
                UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(camera, false);
            }
        }

        protected override InputDeviceCharacteristics DesiredCharacteristics => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;
    }
}
