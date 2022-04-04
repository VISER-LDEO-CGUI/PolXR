using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarEvents : MonoBehaviour, IMixedRealityPointerHandler
{
    // Pop up menu and the mark object.
    public GameObject Menu;
    public GameObject MarkObj;

    // Measurement tool
    public GameObject MeasureObj;
    public GameObject line;

    // The file root under the "Resources" folder.
    public string fileRoot = "Radar Images";
    public Texture defaultText;

    // The transparency value.
    private float alpha = 1.0f;

    // Keep the scales within range.
    private float scaleX, scaleY, scaleZ;
    private float[] scaleRange = { 0.5f, 1.5f };

    // Return the original scale.
    public Vector3 GetScale() { return new Vector3(scaleX, scaleY, scaleZ); }

    // The original transform.
    private Vector3 position;
    private Vector3 rotation;

    // The line scale and assigned or not.
    private Vector3 LineScale;
    private Transform CSVLine = null;

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

        scaleX = this.transform.localScale.x;
        scaleY = this.transform.localScale.y;
        scaleZ = this.transform.localScale.z;
        position = this.transform.position;
        rotation = this.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update() {}

    // Show the menu and mark and update the variables.
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        SychronizeMenu();
        
        // Measurement
        if (Menu.GetComponent<MenuEvents>().MeasureMode && (MarkObj.transform.parent == this.transform))
        {
            MeasureObj.SetActive(true);
            MeasureObj.transform.rotation = this.transform.rotation;
            MeasureObj.transform.SetParent(this.transform);
            MeasureObj.transform.position = eventData.Pointer.Result.Details.Point;
            line.SetActive(true);
        } else {
            // Clean up!
            line.SetActive(false);
            MeasureObj.SetActive(false);
            // The mark.
            MarkObj.SetActive(true);
            MarkObj.transform.rotation = this.transform.rotation;
            MarkObj.transform.SetParent(this.transform);
            MarkObj.transform.position = eventData.Pointer.Result.Details.Point;
        }

    }

    // Unused functions.
    public void OnPointerDragged(MixedRealityPointerEventData eventData) {}
    public void OnPointerUp(MixedRealityPointerEventData eventData) {}
    public void OnPointerClicked(MixedRealityPointerEventData eventData) {}

    // Change the transparancy of the radar images.
    public void SetAlpha(float newAlpha)
    {
        alpha = newAlpha;
        transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
    }

    // Assign the line to the radar image.
    public void SetLine(Transform line)
    {
        line.parent = this.transform;
        line.name = "Line";
        CSVLine = line;
        LineScale = line.localScale;
    }

    // Reset the radar shape.
    public void ResetRadar()
    {
        this.transform.position = position;
        this.transform.rotation = Quaternion.Euler(rotation);
        this.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        SetAlpha(1);
        ToggleRadar(true);
    }

    public void ToggleRadar(bool toggle)
    {
        this.transform.GetComponent<BoxCollider>().enabled = toggle;
        this.transform.GetComponent<BoundsControl>().enabled = toggle;
        transform.GetChild(0).gameObject.SetActive(toggle);
        transform.GetChild(1).gameObject.SetActive(toggle);
        MarkObj.gameObject.SetActive(toggle);
    }

    // Toggle the line.
    public void ToggleLine(bool toggle)
    {
        if (CSVLine) CSVLine.localScale = toggle ? LineScale : new Vector3(0, 0, 0);
    }

    public void SychronizeMenu()
    {
        // The menu.
        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;
        Menu.transform.GetComponent<MenuEvents>().ResetRadar(this.transform, newPosition, alpha);
        Menu.transform.GetComponent<MenuEvents>().CloseButton(false);

        // Constrain the scales.
        Vector3 scale = this.transform.localScale;
        scale.x = scale.x > scaleX * scaleRange[1] ? scaleX * scaleRange[1] : scale.x;
        scale.x = scale.x < scaleX * scaleRange[0] ? scaleX * scaleRange[0] : scale.x;
        scale.y = scale.y > scaleY * scaleRange[1] ? scaleY * scaleRange[1] : scale.y;
        scale.y = scale.y < scaleY * scaleRange[0] ? scaleY * scaleRange[0] : scale.y;
        scale.z = scaleZ;
        this.transform.localScale = scale;
        Menu.transform.GetComponent<MenuEvents>().ConstraintSlider(scale.x / scaleX - 0.5f, scale.y / scaleY - 0.5f);
    }
}