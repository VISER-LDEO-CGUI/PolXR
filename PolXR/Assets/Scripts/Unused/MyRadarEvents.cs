using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Microsoft.MixedReality.Toolkit.Input;

public class MyRadarEvents : MonoBehaviour//, IMixedRealityPointerHandler
{
    //public void OnPointerDown(MixedRealityPointerEventData eventData)
    //{
    //    ToggleRadar(true);
    //}
    //public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    //public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    //public void OnPointerUp(MixedRealityPointerEventData eventData) { }

    private void ToggleRadar(bool state)
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Data"))
            {
                child.gameObject.SetActive(!child.gameObject.activeSelf);
            }
        }
    }
}
