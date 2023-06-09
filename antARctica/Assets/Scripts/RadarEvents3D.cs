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

    // The scientific objects
    public GameObject radargrams;
    public GameObject flightline;

    // The minimap objects
    public GameObject radarMark;
    private Vector3 newPointPos;
    private Color markColor;

    // Object state
    private bool loaded = false;
    private bool selected = false;

    // Start is called before the first frame update
    void Start()
    {
        // Store initial values
        scaleX = this.transform.localScale.x;
        scaleY = this.transform.localScale.y;
        scaleZ = this.transform.localScale.z;
        position = this.transform.localPosition;
        rotation = this.transform.eulerAngles;

        // Grab relevant objects
        flightline = this.transform.GetChild(1).gameObject;
        radargrams = this.transform.GetChild(2).gameObject;
        radarMark = this.transform.GetChild(3).gameObject;

        // Set objects to their starting states
        radarMark.SetActive(false);
        TogglePolyline(true);
        ToggleRadar(false);
    }

    public void TogglePolyline(bool toggle)
    {
        // Actually toggle the polyline
        flightline.SetActive(toggle);
        if (!toggle)
        {
            loaded = false;
            return;
        }

        // Render line based on inputs
        LineRenderer lineRenderer = flightline.GetComponent<LineRenderer>();

        // Set color based on selection
        lineRenderer.startColor = lineRenderer.endColor = loaded ?
            selected ? 
                new Color(1f, 0f, 0f)       // loaded and selected
                : new Color(1f, .4f, 0f)    // loaded, not selected
            : new Color(1f, 1f, 0f);        // not loaded

        // Set width based on highlight
        //lineRenderer.startWidth = lineRenderer.endWidth = highlight ? 0.1f : 0.05f;
    }

    // Turn on/off the 3D surfaces and associated colliders
    public void ToggleRadar(bool toggle)
    {
        this.transform.GetComponent<BoxCollider>().enabled = toggle;
        this.transform.GetComponent<BoundsControl>().enabled = toggle;
        radargrams.SetActive(toggle);
        loaded = toggle;
        //MarkObj.gameObject.SetActive((MarkObj.transform.parent == this.transform) && toggle);
    }

    // Show the menu and mark and update the variables
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // Select the 
        Debug.Log("Clicked " + flightline.name);
        if (loaded) selected = true;

        // Update the menu
        //SychronizeMenu();

        // Only load the images when selected
        ToggleRadar(true);

        // Select the flightline portion
        TogglePolyline(true);

        // Measurement
        
    }

    // Unused functions
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    private void LateUpdate() { }

    // Change the transparancy of the radar images. "onlyLower" used for setting radar only to more transparent level
    public void SetAlpha(float newAlpha, bool onlyLower=false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        for (int i = 0; i < 2; i++)
        {
            radargrams.transform.GetChild(i).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        }
    }

    // Reset the radar shape
    public void ResetRadar(bool whiten)
    {
        // Return the radargrams to their original position
        radargrams.transform.localPosition = position;
        radargrams.transform.localRotation = Quaternion.Euler(rotation);
        radargrams.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        // Unselect the object
        selected = false;

        // Turn the radargrams off
        ToggleRadar(false);

        // Ensure the flightline is still on
        TogglePolyline(true);

        // Turn off the radar mark
        radarMark.SetActive(false);
    }

    // Sychronize the parameters for the main/radar menu
    public void SychronizeMenu()
    {
        // Snap the menu to in front of the user
        Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;

        // Set the buttons
        Menu.transform.GetComponent<MenuEvents>().CloseButton(false);
        Menu.transform.GetComponent<MenuEvents>().ResetRadarSelected(this.transform, newPosition, alpha);
        Menu.transform.GetComponent<MenuEvents>().syncScaleSlider();

        // Turn on the radar mark
        radarMark.SetActive(true);
    }
}