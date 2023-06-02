using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
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
    private bool loaded = false;

    // The transparency value.
    private float alpha = 1.0f;

    // Keep the original scale.
    private float scaleX, scaleY, scaleZ;

    // Return the original scale.
    public Vector3 GetScale() { return new Vector3(scaleX, scaleY, scaleZ); }

    // The original transform.
    private Vector3 position;
    private Vector3 rotation;

    // The line scale and assigned or not.
    private Vector3 LineScale;
    private int DotCount = 0;
    private Transform CSVLine = null;

    // The mark shown on the minimap
    public GameObject radarMark;

    private bool newPointAdded = false;
    private Vector3 newPointPos;
    private Color markColor;

    // Start is called before the first frame update
    void Start()
    {
        // Get and set the texture of the radar image object.
        defaultText = Resources.Load<Texture2D>(fileRoot + "/white");
        loadImage(defaultText);

        // Turn off marks initially.
        radarMark.SetActive(false);

        scaleX = this.transform.localScale.x;
        scaleY = this.transform.localScale.y;
        scaleZ = this.transform.localScale.z;
        position = this.transform.localPosition;
        rotation = this.transform.eulerAngles;
    }

    // For adding particle. Late update needed because the initialization of particle system takes time.
    private void LateUpdate()
    {
        if (newPointAdded)
        {
            var main = CSVLine.GetComponent<ParticleSystem>().main;
            ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[main.maxParticles];
            CSVLine.GetComponent<ParticleSystem>().GetParticles(CSVPoints);

            // Set the particle format.
            CSVPoints[main.maxParticles - 1] = CSVPoints[0];
            CSVPoints[main.maxParticles - 1].position = newPointPos;
            CSVPoints[main.maxParticles - 1].startColor = markColor;

            // Set the new particle.
            CSVLine.GetComponent<ParticleSystem>().SetParticles(CSVPoints, main.maxParticles);

            newPointAdded = false;
        }
    }

    // Dynamically load images after the radar image is selected/deselected.
    private void loadImage(Texture content)
    {
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

    // Show the menu and mark and update the variables.
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        SychronizeMenu();

        // Only load the images when selected.
        Texture content = Resources.Load<Texture2D>(fileRoot + '/' + this.transform.name);
        loadImage(content);
        loaded = true;

        // Measurement
        if (Menu.GetComponent<MenuEvents>().measureMode() == 1)
        {
            MeasureObj.SetActive(true);
            MeasureObj.transform.rotation = this.transform.rotation;
            MeasureObj.transform.SetParent(this.transform);
            MeasureObj.transform.position = eventData.Pointer.Result.Details.Point;
            line.SetActive(true);
        }
        else
        {
            if (Menu.GetComponent<MenuEvents>().measureMode() == 0)
            {
                // Clean up!
                line.SetActive(false);
                MeasureObj.SetActive(false);
            }

            // The mark.
            MarkObj.SetActive(true);
            MarkObj.transform.rotation = this.transform.rotation;
            MarkObj.transform.SetParent(this.transform);
            MarkObj.transform.position = eventData.Pointer.Result.Details.Point;
        }
    }

    // Unused functions.
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

    // Dynamic loading.
    public void TempLoad(bool loadNew)
    {
        if (!loaded)
        {
            if (loadNew)
            {
                Texture content = Resources.Load<Texture2D>(fileRoot + '/' + this.transform.name);
                loadImage(content);
            }
            else
            {
                defaultText = Resources.Load<Texture2D>(fileRoot + "/white");
                loadImage(defaultText);
            }
        }
    }

    // Change the transparancy of the radar images. "onlyLower" used for setting radar only to more transparent level.
    public void SetAlpha(float newAlpha, bool onlyLower = false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
    }

    // Assign the csv line to the radar image.
    public void SetLine(Transform inputLine, int inputCount)
    {
        DotCount += inputCount;
        if (CSVLine)
        {
            // Merge the two lines.
            ParticleSystem originalLine = CSVLine.GetComponent<ParticleSystem>();
            var main = originalLine.main;
            ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[DotCount];
            originalLine.GetParticles(CSVPoints);

            ParticleSystem newLine = inputLine.GetComponent<ParticleSystem>();
            ParticleSystem.Particle[] newPoints = new ParticleSystem.Particle[inputCount];
            newLine.GetParticles(newPoints);
            for (int i = 1; i <= inputCount; i++) CSVPoints[DotCount - i] = newPoints[inputCount - i];

            main.maxParticles = DotCount;
            originalLine.Emit(inputCount);
            originalLine.SetParticles(CSVPoints, DotCount + inputCount);
        }
        else
        {
            inputLine.parent = this.transform;
            inputLine.name = "Line";
            CSVLine = inputLine;
            LineScale = inputLine.localScale;
        }
    }

    // Reset the radar shape.
    public void ResetRadar(bool whiten)
    {
        this.transform.localPosition = position;
        this.transform.localRotation = Quaternion.Euler(rotation);
        this.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        SetAlpha(1);
        if (whiten) ToggleRadar(true);

        // Load place holder image when reset.
        if (whiten && MarkObj.transform.parent != this.transform)
        {
            defaultText = Resources.Load<Texture2D>(fileRoot + "/white");
            loadImage(defaultText);
            loaded = false;
            radarMark.SetActive(false);
        }
    }

    // Add a point in the line.
    public void AddNewPoint(Color inputColor)
    {
        if (CSVLine && MarkObj.activeSelf && MarkObj.transform.parent == this.transform)
        {
            // Transform the position into particle coordinates.
            newPointPos = MarkObj.transform.position - CSVLine.transform.position;
            newPointPos = Quaternion.Euler(0, -this.transform.localEulerAngles.y, 0) * newPointPos;
            newPointPos.x /= CSVLine.lossyScale.x;
            newPointPos.y /= CSVLine.lossyScale.y;
            newPointPos.z /= CSVLine.lossyScale.z;

            markColor = inputColor;

            // Emit a new particle.
            var main = CSVLine.GetComponent<ParticleSystem>().main;
            main.maxParticles += 1;
            CSVLine.GetComponent<ParticleSystem>().Emit(1);

            // Append for position setting.
            newPointAdded = true;
        }
    }

    // Delete points marked from the csv line (not used currently, need to add buttons)
    public void UndoAddPoint(bool UndoAll)
    {
        ParticleSystem originalLine = CSVLine.GetComponent<ParticleSystem>();
        var main = originalLine.main;
        if (main.maxParticles > DotCount)
        {
            main.maxParticles = UndoAll ? DotCount : main.maxParticles - 1;
            ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[main.maxParticles];
            originalLine.GetParticles(CSVPoints);
            originalLine.SetParticles(CSVPoints, main.maxParticles);
        }
    }

    // Turn on/off the image itself.
    public void ToggleRadar(bool toggle)
    {
        this.transform.GetComponent<BoxCollider>().enabled = toggle;
        this.transform.GetComponent<BoundsControl>().enabled = toggle;
        transform.GetChild(0).gameObject.SetActive(toggle);
        transform.GetChild(1).gameObject.SetActive(toggle);
        MarkObj.gameObject.SetActive((MarkObj.transform.parent == this.transform) && toggle);
    }

    // Toggle the line.
    public void ToggleLine(bool toggle)
    {
        if (CSVLine) CSVLine.localScale = toggle ? LineScale : new Vector3(0, 0, 0);
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