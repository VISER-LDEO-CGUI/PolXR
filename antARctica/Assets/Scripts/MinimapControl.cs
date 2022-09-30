using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class MinimapControl : MonoBehaviour, IMixedRealityPointerHandler
{
    // The position information of the scene and user.
    public Transform Antarctica;
    public Transform User;

    // Synchronize between main scene and minimap.
    public Vector3 Offset = new Vector3(0.01f, 0.042f, 0.026f);
    public Vector3 Scale = new Vector3(0.005f, 0.005f, 0);
    private Vector3 PosVec;
    private Vector3 TransVec;
    public Material Normal;
    public Material Outofbound;
    public Material Translate;

    // The red dot on minimap.
    public GameObject PositionObj;
    public Transform Anchor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Compute and update the current user position relative to the scene;
        PosVec = Antarctica.transform.InverseTransformPoint(User.transform.position);
        PosVec.y = PosVec.z;
        PosVec = Vector3.Scale(PosVec, Scale);
        PosVec += Offset;

        // Make sure the dot does not go out of the bounding area.
        if (this.GetComponent<BoxCollider>().enabled) PositionObj.GetComponent<MeshRenderer>().material = Translate;
        else if (Mathf.Abs(PosVec.x) < 0.05f && Mathf.Abs(PosVec.y) < 0.0375f) PositionObj.GetComponent<MeshRenderer>().material = Normal;
        else PositionObj.GetComponent<MeshRenderer>().material = Outofbound;

        // Ceil and floor the value.
        if (PosVec.x >= 0.05f) PosVec.x = 0.05f;
        else if (PosVec.x <= -0.05f) PosVec.x = -0.05f;
        if (PosVec.y >= 0.0375f) PosVec.y = 0.0375f;
        else if (PosVec.y <= -0.0375f) PosVec.y = -0.0375f;

        PositionObj.transform.localPosition = PosVec;
    }

    // Translate to the target point.
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // Compute the real offset.
        TransVec = eventData.Pointer.Result.Details.Point;
        TransVec = this.transform.InverseTransformPoint(TransVec);
        TransVec -= Offset;
        TransVec.x = TransVec.x / Scale.x;
        TransVec.z = TransVec.y / Scale.y;
        TransVec.y = 0;

        // Translate.
        Anchor.localPosition = TransVec;
        MixedRealityPlayspace.Transform.Translate(Anchor.position);
    }

    // Unused functions.
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
}
