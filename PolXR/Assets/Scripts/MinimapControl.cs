using System.Collections;
using System.Collections.Generic;
//using Microsoft.MixedReality.Toolkit;
//using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapControl : MonoBehaviour//, IMixedRealityPointerHandler
{
    // The position information of the scene and user.
    public Transform Location;
    public Transform User;
    public Camera MinimapCamera;
    public Vector3 MapCamPosition = new Vector3(-12.5f, 100f, -90f);
    public float ViewSize = 60f;

    // Synchronize between main scene and minimap.
    public float userHeight = 5;
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
        string sceneName = SceneManager.GetActiveScene().name == "antarctica" ? "Antarctica" : "Petermann";
        Location = GameObject.Find(sceneName).transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Set the camera position to capture the correct view.
        MinimapCamera.transform.eulerAngles = new Vector3(90, Location.transform.eulerAngles.y, 0);
        Vector3 OffsetScaled = MapCamPosition * Location.localScale.x;
        OffsetScaled.y = MapCamPosition.y;
        MinimapCamera.transform.position = Location.transform.position + Quaternion.Euler(0, Location.transform.eulerAngles.y, 0) * OffsetScaled;
        MinimapCamera.orthographicSize = ViewSize * Location.localScale.x;

        // Make sure the dot does not go out of the bounding area.
        if (this.GetComponent<BoxCollider>().enabled) PositionObj.GetComponent<MeshRenderer>().material = Translate;
        else PositionObj.GetComponent<MeshRenderer>().material = Normal;

        // Setting the height of the mark.
        Vector3 newPosition = PositionObj.transform.parent.position;
        newPosition.y = Location.position.y + MapCamPosition.y * 0.9f;
        PositionObj.transform.position = newPosition;
        PositionObj.transform.eulerAngles = new Vector3(90, 0, 0);
    }

    // Translate to the target point.
    //public void OnPointerDown(MixedRealityPointerEventData eventData)
    //{
    //    // Compute the real offset.
    //    TransVec = eventData.Pointer.Result.Details.Point;
    //    TransVec = this.transform.InverseTransformPoint(TransVec);

    //    // X and Y axis are different lengths. X is longer than Y.
    //    TransVec.x *= (this.transform.localScale.x / this.transform.localScale.y) * (2 * ViewSize);
    //    TransVec.z = TransVec.y * (2 * ViewSize);

    //    // Offset
    //    TransVec += MapCamPosition;

    //    TransVec.y = userHeight;

    //    // Translate.
    //    Anchor.localPosition = TransVec;
    //    MixedRealityPlayspace.Transform.Translate(Anchor.position - User.position);
    //}

    // Unused functions.
    //public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    //public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    //public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
}
