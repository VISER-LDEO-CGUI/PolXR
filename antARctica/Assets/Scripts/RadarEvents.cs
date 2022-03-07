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

    // The transparency value.
    private float alpha = 1.0f;

    // Keep the scales within range.
    private float scaleX;
    private float scaleY;
    private float scaleZ;
    private float[] scaleRange = { 0.5f, 1.5f };

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
    }

    // Update is called once per frame
    void Update()
    {
        // Constrain the scales.
        bool exceed = false;
        Vector3 scale = this.transform.localScale;
        if (scale.x > scaleX * scaleRange[1])
        {
            scale.x = scaleX * scaleRange[1];
            exceed = true;
        }
        else if (scale.x < scaleX * scaleRange[0])
        {
            scale.x = scaleX * scaleRange[0];
            exceed = true;
        }
        if (scale.y > scaleY * scaleRange[1])
        {
            scale.y = scaleY * scaleRange[1];
            exceed = true;
        }
        else if (scale.y < scaleY * scaleRange[0])
        {
            scale.y = scaleY * scaleRange[0];
            exceed = true;
        }
        scale.z = scaleZ;

        if (exceed) this.transform.localScale = scale;
    }

    // Show the menu and mark and update the variables.
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // The menu.
        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;
        Menu.transform.GetComponent<MenuEvents>().ResetRadar(this.transform, newPosition, alpha);
        Menu.transform.GetComponent<MenuEvents>().CloseButton(false);

        // The mark.
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

    public void SetAlpha(float newAlpha)
    {
        alpha = newAlpha;
        transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
    }
}