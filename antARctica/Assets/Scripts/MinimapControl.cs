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
    public Camera MinimapCamera;
    public Vector3 MapCamPosition = new Vector3(-12.5f, 100f, -90f);
    public float ViewSize = 60f;

    // Synchronize between main scene and minimap.
    public Vector3 Offset = new Vector3(0.01f, 0.042f, 0.026f);
    public Vector3 Scale = new Vector3(0.005f, 0.005f, 0);
    private Vector3 PosVec;
    private Vector3 TransVec;
    public Material Normal;
    public Material Translate;

    // The red dot on minimap.
    public GameObject PositionObj;
    public Transform Anchor;

    // Start is called before the first frame update
    void Start()
    {
        PositionObj.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // Set the camera position to capture the correct view.
        Vector3 OffsetScaled = MapCamPosition * Antarctica.localScale.x;
        OffsetScaled.y = MapCamPosition.y;
        MinimapCamera.transform.position = Antarctica.transform.position + OffsetScaled;
        MinimapCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        MinimapCamera.orthographicSize = ViewSize * Antarctica.localScale.x;

        // Compute and update the current user position relative to the scene;
        PosVec = Antarctica.transform.InverseTransformPoint(User.transform.position);
        PosVec.y = PosVec.z;
        PosVec = Vector3.Scale(PosVec, Scale);
        PosVec += Offset;

        // Make sure the dot does not go out of the bounding area.
        if (this.GetComponent<BoxCollider>().enabled) PositionObj.GetComponent<MeshRenderer>().material = Translate;
        else PositionObj.GetComponent<MeshRenderer>().material = Normal;

        // Setting the height of the mark.
        Vector3 newPosition = PositionObj.transform.parent.position;
        newPosition.y = Antarctica.position.y + MapCamPosition.y * 0.9f;
        PositionObj.transform.position = newPosition;
        PositionObj.transform.eulerAngles = new Vector3(0, 0, 0);
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
