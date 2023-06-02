using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

public class Measurement : MonoBehaviour, IMixedRealityPointerHandler
{
    public void Start() { }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        /*
                if (Menu.GetComponent<MenuEvents>().measureMode() == 1)
                {
                    MeasureObj.SetActive(true);
                    MeasureObj.transform.rotation = this.transform.rotation;
                    MeasureObj.transform.SetParent(this.transform);
                    MeasureObj.transform.position = eventData.Pointer.Result.Details.Point;
                    line.SetActive(true);
                }
                else
                {
                    if (Menu.GetComponent<MenuEvents>().measureMode() == 0)
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
                */
    }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
}