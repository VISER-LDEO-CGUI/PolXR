using Fusion.XR.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.XR;

/*
 * Workaround the OpenXR 1.9.1 bug regarding the floor tracking origine, see https://forum.unity.com/threads/update-com-unity-xr-openxr-from-1-8-2-to-1-9-1-changes-tracking-origin.1515263/
 */
public class ResetTrackingOrigin : MonoBehaviour
{
    public float fixDuration = 3;
    float fixElapsedTime = 0;

    public List<string> unityVersionRequiringFixing = new List<string> { "2021.3.32f1" };
#if ENABLE_INPUT_SYSTEM
    public InputActionProperty headsetAvailableAction = new InputActionProperty();
#endif
    public enum Status
    {
        Unknown,
        FixNotRequired,
        FixRequiredHeadsetNotDetected,
        FixBeingApplied,
        FixFinished
    }
    public Status status = Status.Unknown;

    public bool HeadsetDetected
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            InputTrackingState trackingState = (InputTrackingState)headsetAvailableAction.action.ReadValue<int>();
            return trackingState != InputTrackingState.None;
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        headsetAvailableAction.EnableWithDefaultXRBindings(new List<string> { "<XRHMD>/trackingState" });
#else
        Debug.LogError("Missing com.unity.inputsystem package");
#endif
    }

    private void Start()
    {
        if (unityVersionRequiringFixing.Contains(Application.unityVersion))
        {
            Debug.LogError($"[ResetTrackingOrigin] The <a href=\"https://forum.unity.com/threads/update-com-unity-xr-openxr-from-1-8-2-to-1-9-1-changes-tracking-origin.1515263/\">OpenXR 1.9.1 floor tracking issue</a> fix is required for this version of Unity ({Application.unityVersion}). The XR Rig TrackingOriginMode will be tweak for {(int)fixDuration} seconds.");
            Debug.Log("Looking for headset...");
            status = Status.FixRequiredHeadsetNotDetected;
        }
        else
        {
            Debug.LogWarning($"[ResetTrackingOrigin] The <a href=\"https://forum.unity.com/threads/update-com-unity-xr-openxr-from-1-8-2-to-1-9-1-changes-tracking-origin.1515263/\">OpenXR 1.9.1 floor tracking issue</a> fix is not know to be required for this version of Unity ({Application.unityVersion}). If you're camera appears in the floot, add this version of Unity to ResetTrackingOrigin unityVersionRequiringFixing list of version to fix.");
            status = Status.FixNotRequired;
        }
    }

    private void Update()
    {
        if (status == Status.FixRequiredHeadsetNotDetected && HeadsetDetected)
        {
            Debug.Log("Floor tracking origin fix start");
            status = Status.FixBeingApplied;
        }
        if (status != Status.FixBeingApplied) return;
        if (fixElapsedTime >= fixDuration)
        {
            fixElapsedTime = -1;
            Debug.Log("Floor tracking origin fix stop");
            status = Status.FixFinished;
        }
        else
        {
            FixfloorOrigin();
            fixElapsedTime += Time.deltaTime;
        }
    }
    private void FixfloorOrigin()
    {
        List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>(inputSubsystems);
        foreach (var inputSubsystem in inputSubsystems)
        {
            // change the tracking oroigin to set it to floor
            if (inputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device)) { }
            if (inputSubsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor)) { }
        }
    }
}
