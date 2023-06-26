using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

public class RadarEvents3D : RadarEvents, IMixedRealityPointerHandler
{

    // The scientific objects
    public GameObject radargrams;
    public GameObject flightline;

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
    public new void ToggleRadar(bool toggle)
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
    public new void SetAlpha(float newAlpha, bool onlyLower=false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        for (int i = 0; i < 2; i++)
        {
            radargrams.transform.GetChild(i).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        }
    }

    // Reset the radar shape
    public new void ResetRadar(bool whiten)
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

}