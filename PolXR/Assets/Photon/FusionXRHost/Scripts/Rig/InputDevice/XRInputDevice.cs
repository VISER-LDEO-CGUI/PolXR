using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/***
 * 
 * XRInputDevice detects the XR input devices and update the transform with actual position/rotation of the device if shouldSynchDevicePosition boolean is set to true
 * 
 ***/

namespace Fusion.XR.Host.Rig
{
    public class XRInputDevice : MonoBehaviour
    {
        public const int INTERPOLATION_BUFFER_SIZE = 20;

        [Header("Detected input device")]
        [SerializeField] bool isDeviceFound = false;
        public InputDevice device;

        [Header("Synchronisation")]
        public bool shouldSynchDevicePosition = true;

        [Header("Positioning timing")]
        public bool updateOnAfterInputSystemUpdate = true;

        public Fusion.Tools.TimedRingbuffer<InputDevicePose> ringBuffer = new Fusion.Tools.TimedRingbuffer<InputDevicePose>(INTERPOLATION_BUFFER_SIZE);

        InputDevicePose latestPose;

        public struct InputDevicePose: Fusion.Tools.ICopiable<InputDevicePose>
        {
            public Vector3 position;
            public Quaternion rotation;

            public void CopyValuesFrom(InputDevicePose source) {
                position = source.position;
                rotation = source.rotation;
            }

        }

        protected virtual InputDeviceCharacteristics DesiredCharacteristics => InputDeviceCharacteristics.TrackedDevice;

        protected void OnEnable()
        {
            UnityEngine.InputSystem.InputSystem.onAfterUpdate += OnAfterInputSystemUpdate;

        }

        protected void OnDisable()
        {
            UnityEngine.InputSystem.InputSystem.onAfterUpdate -= OnAfterInputSystemUpdate;
        }

        
        public void OnAfterInputSystemUpdate()
        {
            if (updateOnAfterInputSystemUpdate)
            {
                UpdatePosition();
            }
            latestPose.position = transform.position;
            latestPose.rotation = transform.rotation;
            ringBuffer.Add(latestPose, Time.time);
        }

        public InputDevicePose InterpolatedPose(float delay = 0.3f)
        {
            var interpolationInfo = ringBuffer.InterpolateInfo(Time.time - delay);
            switch (interpolationInfo.status)
            {
                case Tools.InterpolationStatus.ValidFrom: return interpolationInfo.from;
                case Tools.InterpolationStatus.ValidTo: return interpolationInfo.to;
                case Tools.InterpolationStatus.ValidFromTo:
                    return new InputDevicePose {
                        position = Vector3.Lerp(interpolationInfo.from.position, interpolationInfo.to.position, interpolationInfo.alpha),
                        rotation = Quaternion.Slerp(interpolationInfo.from.rotation, interpolationInfo.to.rotation, interpolationInfo.alpha)
                    }; 
            }
            return latestPose;
        }

        public virtual void DetectDevice()
        {
            if (isDeviceFound) return;
            foreach (var d in DeviceLookup())
            {
                device = d;
                isDeviceFound = true;
                break;
            }
        }
        public virtual List<InputDevice> DeviceLookup()
        {
            InputDeviceCharacteristics desiredCharacteristics = DesiredCharacteristics;
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, devices);
            return devices;
        }

        protected virtual void Update()
        {
            UpdatePosition();
        }

        protected  void UpdatePosition()
        { 
            if (shouldSynchDevicePosition)
            {
                DetectDevice();
                if (isDeviceFound && device.TryGetFeatureValue(CommonUsages.devicePosition, out var position))
                {
                    transform.localPosition = position;
                }
                if (isDeviceFound && device.TryGetFeatureValue(CommonUsages.deviceRotation, out var rotation))
                {
                    transform.localRotation = rotation;
                }
            }
        }
    }
}
