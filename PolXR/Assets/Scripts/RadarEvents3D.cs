using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

public class RadarEvents3D : RadarEvents, IMixedRealityPointerHandler
{

    // The scientific objects
    public GameObject radargrams;
    public GameObject flightline;

    // Start is called before the first frame update
    void Start()
    {

        // Grab relevant objects
        flightline = this.transform.GetChild(1).gameObject;
        radargrams = this.transform.GetChild(2).gameObject;
        radarMark = this.transform.GetChild(3).gameObject;

        // Store initial values
        scaleX = radargrams.transform.localScale.x;
        scaleY = radargrams.transform.localScale.y;
        scaleZ = radargrams.transform.localScale.z;
        position = radargrams.transform.localPosition;
        rotation = radargrams.transform.eulerAngles;

        // Add manipulation listeners to the radargrams
        radargrams.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().OnManipulationStarted.AddListener(Select);

        // Set objects to their starting states
        radarMark.SetActive(false);
        TogglePolyline(true, false);
        ToggleRadar(false);
    }

    public void TogglePolyline(bool toggle, bool selectRadar)
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
            selectRadar ? 
                new Color(1f, 0f, 0f)       // loaded and selected
                : new Color(1f, .4f, 0f)    // loaded, not selected
            : new Color(1f, 1f, 0f);        // not loaded

    }

    // Turn on/off the 3D surfaces and associated colliders
    public new void ToggleRadar(bool toggle)
    {
        this.transform.GetComponent<BoxCollider>().enabled = !loaded;
        this.transform.GetComponent<BoundsControl>().enabled = toggle;
        radargrams.SetActive(toggle);
        loaded = toggle;
        //MarkObj.gameObject.SetActive((MarkObj.transform.parent == this.transform) && toggle);
    }

    public void Select()
    {
        // Update the menu
        SychronizeMenu();

        // Select the flightline portion
        foreach (RadarEvents3D sibling in this.transform.parent.gameObject.GetComponentsInChildren<RadarEvents3D>())
        {
            sibling.TogglePolyline(true, false);
        }
        TogglePolyline(true, true);
    }
    private void Select(ManipulationEventData eventData)
    {
        Select();
    }

    // Show the menu and mark and update the variables
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // Only load the images when selected
        ToggleRadar(true);

        // Show that the object has been selected
        Select();

        // Measurement

    }

    // Unused functions
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    private void LateUpdate() { }

    // Gets the scale of the radargram
    public new Vector3 GetScale()
    {
        return new Vector3(radargrams.transform.localScale.x, radargrams.transform.localScale.y, radargrams.transform.localScale.z);
    }

    // Change the transparency of the radar images. "onlyLower" used for setting radar only to more transparent level
    public new void SetAlpha(float newAlpha, bool onlyLower=false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        for (int i = 0; i < 2; i++)
        {
            Color color = radargrams.transform.GetChild(i).gameObject.GetComponent<Renderer>().material.color;
            color.a = newAlpha;
            Debug.Log($"Set {radargrams.transform.GetChild(i).gameObject.name} to alpha={color.a}");
        }
    }

    // Just resets the radar transform
    public void ResetTransform()
    {
        radargrams.transform.localPosition = position;
        radargrams.transform.localRotation = Quaternion.Euler(rotation);
        radargrams.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    // Reset the radar as if it had not been loaded
    public void ResetRadar()
    {
        // Return the radargrams to their original position
        ResetTransform();

        // Turn the radargrams off
        ToggleRadar(false);

        // Ensure the flightline is still on
        TogglePolyline(true, false);

        // Turn off the radar mark
        radarMark.SetActive(false);
    }

}