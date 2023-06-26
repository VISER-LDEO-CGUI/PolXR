using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RadarEvents : MonoBehaviour
{
    // Variables for determining which workflow is used (2D or 3D)
    int workflow;
    string radarEventsScript;

    // Pop up menu and the mark object.
    public GameObject Menu;
    public GameObject MarkObj;

    // Measurement tool
    public GameObject MeasureObj;
    public GameObject line;

    // The file root under the "Resources" folder.
    protected bool loaded = false;

    // The transparency value.
    protected float alpha = 1.0f;

    // Keep the original scale.
    protected float scaleX, scaleY, scaleZ;

    // The original transform.
    protected Vector3 position;
    protected Vector3 rotation;

    // The mark shown on the minimap
    public GameObject radarMark;
    protected bool newPointAdded = false;
    protected Vector3 newPointPos;
    protected Color markColor;

    void Start()
    {
        GetScene();
        radarEventsScript = "RadarEvents" + (char)workflow + "D";
    }

    // Figure out which other script to call
    public int GetScene()
    {
        workflow = 0;
        switch (SceneManager.GetActiveScene().ToString())
        {
            case "antarctica":
                workflow = 2;
                break;
            case "greenland":
                workflow = 3;
                break;
        }
        return workflow;
    }

    // Return the original scale.
    public Vector3 GetScale() { return new Vector3(scaleX, scaleY, scaleZ); }

    // Turn on/off the image itself.
    public void ToggleRadar(bool toggle) { }

    // Resets the radar parent to its original position.
    public void ResetRadar(bool whiten) { }

    // Change the transparancy of the radar images. "onlyLower" used for setting radar only to more transparent level.
    public void SetAlpha(float newAlpha, bool onlyLower = false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
    }

    // Sychronize the parameters for the main/radar menu.
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