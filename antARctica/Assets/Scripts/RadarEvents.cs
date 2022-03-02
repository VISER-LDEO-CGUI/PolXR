using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarEvents : MonoBehaviour, IMixedRealityPointerHandler
{
    public GameObject Menu;
    public GameObject MarkObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Show the menu and mark and update the variables.
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // The menu
        Menu.transform.GetComponent<UpdateSliders>().ResetRadar(this.transform);
        Menu.transform.position = Vector3.Lerp(eventData.Pointer.Result.Details.Point, Camera.main.transform.position, 0.9f);
        Menu.SetActive(true);

        // The mark
        MarkObj.SetActive(true);
        MarkObj.transform.rotation = this.transform.rotation;
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