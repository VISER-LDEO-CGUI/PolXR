using Fusion.XR.Host.Grabbing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Fusion.XR.Host.Rig 
{
    // Structure representing the inputs driving a hand pose 
    [System.Serializable]
    public struct HandCommand : INetworkStruct
    {
        public float thumbTouchedCommand;
        public float indexTouchedCommand;
        public float gripCommand;
        public float triggerCommand;
        // Optionnal commands
        public int poseCommand;
        public float pinchCommand;// Can be computed from triggerCommand by default
    }

    /**
     * 
     * Hand class for the hardware rig.
     * Handles collecting the input for the hand pose, and the hand interactions
     * 
     **/

    public class HardwareHand : MonoBehaviour
    {
        public RigPart side;
        public HandCommand handCommand;
        public bool isGrabbing = false;
        public Grabber grabber;

        [Header("Hand pose input")]
        public InputActionProperty thumbAction;
        public InputActionProperty gripAction;
        public InputActionProperty triggerAction;
        public InputActionProperty indexAction;
        public int handPose = 0;

        [Header("Hand interaction input")]
        public InputActionProperty grabAction;
        public float grabThreshold = 0.5f;
        //False for Desktop mode, true for VR mode: when the hand grab is triggered by other scripts (MouseTeleport in desktop mode), we do not want to update the isGrabbing. It should only be done in VR mode
        public bool updateGrabWithAction = true;
        public NetworkTransform networkTransform;

        public IHandRepresentation localHandRepresentation;

        private void Awake()
        {
            thumbAction.EnableWithDefaultXRBindings(side: side, new List<string> { "thumbstickTouched", "primaryTouched", "secondaryTouched" });
            gripAction.EnableWithDefaultXRBindings(side: side, new List<string> { "grip" });
            triggerAction.EnableWithDefaultXRBindings(side: side, new List<string> { "trigger" });
            indexAction.EnableWithDefaultXRBindings(side: side, new List<string> { "triggerTouched" });
            gripAction.EnableWithDefaultXRBindings(side: side, new List<string> { "grip" });
            // We separate the hand grip action and the grab interaction action, as we may want to use different action for some hardware
            grabAction.EnableWithDefaultXRBindings(side: side, new List<string> { "grip" });

            grabber = GetComponentInChildren<Grabber>();
            localHandRepresentation = GetComponentInChildren<IHandRepresentation>();
            networkTransform = GetComponent<NetworkTransform>();
        }

        protected virtual void Update()
        {
            // update hand pose
            handCommand.thumbTouchedCommand = thumbAction.action.ReadValue<float>();
            handCommand.indexTouchedCommand = indexAction.action.ReadValue<float>();
            handCommand.gripCommand = gripAction.action.ReadValue<float>();
            handCommand.triggerCommand = triggerAction.action.ReadValue<float>();
            handCommand.poseCommand = handPose;
            handCommand.pinchCommand = 0;

            // update hand interaction
            if(updateGrabWithAction) isGrabbing = grabAction.action.ReadValue<float>() > grabThreshold;
            if (localHandRepresentation != null) localHandRepresentation.SetHandCommand(handCommand);
        }

        #region Haptic feedback (vibrations)
        private UnityEngine.XR.InputDevice? _device = null;
        private bool supportImpulse = false;

        // Find the device associated to a VR controller, to be able to send it haptic feedback (vibrations)
        public UnityEngine.XR.InputDevice? Device
        {
            get
            {
                if (_device == null)
                {
                    InputDeviceCharacteristics sideCharacteristics = side == RigPart.LeftController ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;
                    InputDeviceCharacteristics trackedControllerFilter = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | sideCharacteristics;

                    List<UnityEngine.XR.InputDevice> foundControllers = new List<UnityEngine.XR.InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(trackedControllerFilter, foundControllers);

                    if (foundControllers.Count > 0)
                    {
                        var inputDevice = foundControllers[0];
                        _device = inputDevice;
                        if (inputDevice.TryGetHapticCapabilities(out var hapticCapabilities))
                        {
                            // We memorize if this device can support vibrations
                            supportImpulse = hapticCapabilities.supportsImpulse;
                        }
                    }
                }
                return _device;
            }
        }

        // If a device supporting haptic feedback has been detected, send a vibration to it (here in the form of an impulse)
        public void SendHapticImpulse(float amplitude, float duration, uint channel = 0)
        {
            if (Device != null)
            {
                var inputDevice = Device.GetValueOrDefault();
                if (supportImpulse)
                {
                    inputDevice.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }

        // If a device supporting haptic feedback has been detected, send a vibration to it (here in the form of a buffer describing the vibration data)
        public void SendHapticBuffer(byte[] buffer, uint channel = 0)
        {
            if (Device != null)
            {
                var inputDevice = Device.GetValueOrDefault();
                if (supportImpulse)
                {
                    inputDevice.SendHapticBuffer(channel, buffer);
                }
            }
        }
        #endregion

    }
}
