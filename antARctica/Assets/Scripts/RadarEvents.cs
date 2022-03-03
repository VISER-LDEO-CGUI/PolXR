using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarEvents : MonoBehaviour, IMixedRealityPointerHandler
{
    // Pop up menu and the mark object.
    public GameObject Menu;
    public GameObject MarkObj;

    // The file root under the "Resources" folder.
    public string fileRoot = "Radar Images";
    public Texture defaultText;

    // Start is called before the first frame update
    void Start()
    {
        // Get and set the texture of the radar image object.
        // Need to fix the file path to relative path, or find another way to locate the pictures.
        Texture content = Resources.Load<Texture2D>(fileRoot + '/' + this.transform.name);
        if (content != null)
        {
            transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", content);
            transform.GetChild(1).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", content);
        }
        else
        {
            transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", defaultText);
            transform.GetChild(1).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", defaultText);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Show the menu and mark and update the variables.
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // The menu
        Menu.transform.GetComponent<SliderEvents>().ResetRadar(this.transform);
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