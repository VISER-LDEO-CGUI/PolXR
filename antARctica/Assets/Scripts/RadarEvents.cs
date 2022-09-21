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
    private int DotCount = 0;
    private Transform CSVLine = null;

    // Start is called before the first frame update
    void Start()
    {
        // Get and set the texture of the radar image object.
        defaultText = Resources.Load<Texture2D>(fileRoot + "/white");
        loadImage(defaultText);

        scaleX = this.transform.localScale.x;
        scaleY = this.transform.localScale.y;
        scaleZ = this.transform.localScale.z;
        position = this.transform.localPosition;
        rotation = this.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update() {}

    void loadImage(Texture content)
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
        if (Menu.GetComponent<MenuEvents>().GetMeasureMode() && (MarkObj.transform.parent == this.transform))
        {
            MeasureObj.SetActive(true);
            MeasureObj.transform.rotation = this.transform.rotation;
            MeasureObj.transform.SetParent(this.transform);
            MeasureObj.transform.position = eventData.Pointer.Result.Details.Point;
            line.SetActive(true);
        }
        else
        {
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

    // Change the transparancy of the radar images.
    public void SetAlpha(float newAlpha)
    {
        alpha = newAlpha;
        transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
    }

    // Assign the line to the radar image.
    public void SetLine(Transform line, int inputCount)
    {
        DotCount += inputCount;
        if (CSVLine)
        {
            // Merge the two lines.
            ParticleSystem originalLine = CSVLine.GetComponent<ParticleSystem>();
            var main = originalLine.main;
            ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[DotCount];
            originalLine.GetParticles(CSVPoints);

            ParticleSystem newLine = line.GetComponent<ParticleSystem>();
            ParticleSystem.Particle[] newPoints = new ParticleSystem.Particle[inputCount];
            newLine.GetParticles(newPoints);
            for (int i = 1; i <= inputCount; i++) CSVPoints[DotCount - i] = newPoints[inputCount - i];

            main.maxParticles = DotCount;
            originalLine.Emit(inputCount);
            originalLine.SetParticles(CSVPoints, DotCount + inputCount);
        }
        else
        {
            line.parent = this.transform;
            line.name = "Line";
            CSVLine = line;
            LineScale = line.localScale;
        }
    }

    // Reset the radar shape.
    public void ResetRadar()
    {
        this.transform.localPosition = position;
        this.transform.localRotation = Quaternion.Euler(rotation);
        this.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        SetAlpha(1);
        ToggleRadar(true);

        // Load place holder image when reset.
        if (MarkObj.transform.parent != this.transform)
        {
            defaultText = Resources.Load<Texture2D>(fileRoot + "/white");
            loadImage(defaultText);
            loaded = false;
        }
    }

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
        Menu.transform.GetComponent<MenuEvents>().CloseButton(false);
        Menu.transform.GetComponent<MenuEvents>().ResetRadarSelected(this.transform, newPosition, alpha);

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