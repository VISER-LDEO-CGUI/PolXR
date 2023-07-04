using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

public class Measurement : MonoBehaviour, IMixedRealityPointerHandler
{

    public GameObject Menu;
    public GameObject MarkObj;
    public GameObject MeasureObj;
    public GameObject line;

    private bool measureMode = false;

    public void Start() { }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        measureMode = Menu.GetComponent<MenuEvents>().measureMode() > 0;
        // SetMarkObj(eventData);
    }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void SetMarkObj(MixedRealityPointerEventData eventData)
    {
        if (measureMode)
        {
            MeasureObj.SetActive(true);
            MeasureObj.transform.rotation = this.transform.rotation;
            MeasureObj.transform.SetParent(this.transform);
            MeasureObj.transform.position = eventData.Pointer.Result.Details.Point;
            line.SetActive(true);
        }
        else
        {
            if (!measureMode)
            {
                // Clean up!
                line.SetActive(false);
                MeasureObj.SetActive(false);
            }

            // The mark
            MarkObj.SetActive(true);
            MarkObj.transform.rotation = this.transform.rotation;
            MarkObj.transform.SetParent(this.transform);
            MarkObj.transform.position = eventData.Pointer.Result.Details.Point;
        }
    }
    /*
    public Vector3 CalculateDistance(GameObject a, GameObject b)
    {

    }

    private Vector3 CorrectDistortion(Vector3 a, Vector3 b, int epsg)
    {

    }
    */
}