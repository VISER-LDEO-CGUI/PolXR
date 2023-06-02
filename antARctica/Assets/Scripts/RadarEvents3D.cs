using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

public class RadarEvents3D : MonoBehaviour, IMixedRealityPointerHandler
{

    // Pop up menu and the mark object
    public GameObject Menu;
    public GameObject MarkObj;

    // Measurement tool
    public GameObject MeasureObj;
    public GameObject line;

    // The transparency value
    private float alpha = 1.0f;

    // Keep the original scale
    private float scaleX, scaleY, scaleZ;

    // Return the original scale
    public Vector3 GetScale() { return new Vector3(scaleX, scaleY, scaleZ); }

    // The original transform
    private Vector3 position;
    private Vector3 rotation;

    // The 3D stuff
    public GameObject radargram;
    public GameObject flightline;

    // The minimap objects
    public GameObject radarMark;
    private Vector3 newPointPos;
    private Color markColor;

    // Start is called before the first frame update
    void Start()
    {
        // Store initial values
        scaleX = this.transform.localScale.x;
        scaleY = this.transform.localScale.y;
        scaleZ = this.transform.localScale.z;
        position = this.transform.localPosition;
        rotation = this.transform.eulerAngles;

        // Initialize children properly
        radargram = this.transform.GetChild(0).gameObject;
        flightline = this.transform.GetChild(1).gameObject;
        radarMark = this.transform.GetChild(2).gameObject;

        radarMark.SetActive(false);
        TogglePolyline(true, false);
        ToggleRadar(true);
    }

    public void TogglePolyline(bool toggle, bool selected)
    {
        flightline.SetActive(toggle);

    }

    // Turn on/off the 3D surface
    public void ToggleRadar(bool toggle)
    {
        this.transform.GetComponent<BoxCollider>().enabled = toggle;
        //this.transform.GetComponent<BoundsControl>().enabled = toggle;
        transform.GetChild(0).gameObject.SetActive(toggle);
        //MarkObj.gameObject.SetActive((MarkObj.transform.parent == this.transform) && toggle);
    }

    // Show the menu and mark and update the variables
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        SychronizeMenu();

        // Only load the images when selected
        ToggleRadar(true);

        // Highlight the flightline portion
        TogglePolyline(true, true);

        // Measurement
        
    }

    // Unused functions
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    private void LateUpdate() { }

    // Change the transparancy of the radar images. "onlyLower" used for setting radar only to more transparent level
    public void SetAlpha(float newAlpha, bool onlyLower=false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
    }

    // Reset the radar shape
    public void ResetRadar(bool whiten)
    {
        this.transform.localPosition = position;
        this.transform.localRotation = Quaternion.Euler(rotation);
        this.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        SetAlpha(1);
        ToggleRadar(false);
        TogglePolyline(true, false);

        radarMark.SetActive(false);
    }

    // Sychronize the parameters for the main/radar menu
    public void SychronizeMenu()
    {
        // The menu.
        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;
        Menu.transform.GetComponent<MenuEvents>().CloseButton(false);
        Menu.transform.GetComponent<MenuEvents>().ResetRadarSelected(this.transform, newPosition, alpha);
        Menu.transform.GetComponent<MenuEvents>().syncScaleSlider();
        radarMark.SetActive(true);
    }
}