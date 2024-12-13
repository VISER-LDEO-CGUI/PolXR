using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Fusion.XR.Shared.Grabbing;

[RequireComponent(typeof(NetworkGrabbable))]
public class GrabbableCube : NetworkBehaviour
{
    public TextMeshProUGUI authorityText;
    public TextMeshProUGUI debugText;

    private void Start()
    {
        if (debugText)
            debugText.text = "";
        else
            Debug.LogError("Missing text target for debug");
        if (authorityText)
            authorityText.text = "";
        else
            Debug.LogError("Missing text target for authorityText");
        var grabbable = GetComponent<NetworkGrabbable>();
        grabbable.onDidGrab.AddListener(OnDidGrab);
        grabbable.grabbable.onWillGrab.AddListener(OnWillGrab);
        grabbable.onDidUngrab.AddListener(OnDidUngrab);
    }

    private void DebugLog(string debug)
    {
        if(debugText)
            debugText.text = debug;
        Debug.Log(debug);
    }

    private void UpdateStatusCanvas()
    {
        if (Object.HasStateAuthority)
            authorityText.text = "You have the state authority on this object";
        else
            authorityText.text = "You have NOT the state authority on this object";
    }

    public override void Render()
    {
        base.Render();
        UpdateStatusCanvas();
    }

    void OnDidUngrab()
    {
        DebugLog($"{gameObject.name} ungrabbed");
    }

    void OnWillGrab(Grabber newGrabber)
    {
        DebugLog($"Grab on {gameObject.name} requested by {newGrabber}. Waiting for state authority ...");
    }

    void OnDidGrab(NetworkGrabber newGrabber)
    {
        DebugLog($"{gameObject.name} grabbed by {newGrabber}");
    }
}
