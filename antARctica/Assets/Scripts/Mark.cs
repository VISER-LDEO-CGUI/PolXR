using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mark : MonoBehaviour, IMixedRealityPointerHandler
{
    public GameObject MarkObj;

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        MarkObj.SetActive(true);
        MarkObj.transform.SetParent(this.transform);
        MarkObj.transform.position = eventData.Pointer.Result.Details.Point;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }

}