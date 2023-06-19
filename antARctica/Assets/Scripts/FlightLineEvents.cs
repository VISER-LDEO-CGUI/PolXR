using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

public class FlightLineEvents : MonoBehaviour
{

    public GameObject flightline;

    // Object state
    private bool loaded = false;
    private bool selected = false;

    void Start() 
    {
        flightline = this.gameObject;
    }

	void Update() { }

    

    // Show the menu and mark and update the variables
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // Select the 
        Debug.Log("Clicked " + flightline.name);
        if (loaded) selected = true;

        // Update the menu
        //SychronizeMenu();

        // Only load the images when selected
        //ToggleRadar(true);

        // Select the flightline portion
        TogglePolyline(true);

        // Measurement

    }

    // Unused functions
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    private void LateUpdate() { }

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

}
