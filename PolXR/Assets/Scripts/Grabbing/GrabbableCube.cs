using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Fusion.XR.Host.Grabbing;

[RequireComponent(typeof(NetworkGrabbable))]
public class GrabbableCube : NetworkBehaviour
{
    public TextMeshProUGUI authorityText;
    public TextMeshProUGUI debugText;

    private void Awake()
    {
        debugText.text = "";
        var grabbable = GetComponent<NetworkGrabbable>();
        grabbable.onDidGrab.AddListener(OnDidGrab);
        grabbable.onDidUngrab.AddListener(OnDidUngrab);
    }

    private void DebugLog(string debug)
    {
        debugText.text = debug;
        Debug.Log(debug);
    }

    private void UpdateStatusCanvas()
    {
        if (Object.HasInputAuthority)
            authorityText.text = "You have the input authority on this object";
        else
            authorityText.text = "You have NOT the input authority on this object";

        if (Object.HasStateAuthority)
            authorityText.text += "\nYou have the state authority on this object";
        else
            authorityText.text += "\nYou have NOT the state authority on this object";
    }

    public override void FixedUpdateNetwork()
    {
        UpdateStatusCanvas();
    }

    void OnDidUngrab()
    {
        DebugLog($"{gameObject.name} ungrabbed");
    }

    void OnDidGrab(NetworkGrabber newGrabber)
    {
        DebugLog($"{gameObject.name} grabbed by {newGrabber.Object.InputAuthority} {newGrabber.hand.side} hand");
    }
}
