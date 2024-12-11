using UnityEngine;

public class RadarEvents2D : RadarEvents// IMixedRealityPointerHandler
{

    // The file root under the "Resources" folder.
    public string fileRoot = "Radar Images";
    public Texture defaultText;

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
        // https://docs.unity3d.com/ScriptReference/Component.GetComponents.html
    }

    //public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    //public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    //public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

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
    //public void OnPointerDown(MixedRealityPointerEventData eventData)
    //{
    //    SychronizeMenu();
    //    Debug.Log(eventData.Pointer.Result.Details.Point);
    //    // Only load the images when selected.
    //    Texture content = Resources.Load<Texture2D>(fileRoot + '/' + this.transform.name);
    //    loadImage(content);
    //    loaded = true;

    //    // Measurement
    //    if (Menu.GetComponent<MenuEvents>().measureMode() == 1)
    //    {
    //        MeasureObj.SetActive(true);
    //        MeasureObj.transform.rotation = this.transform.rotation;
    //        MeasureObj.transform.SetParent(this.transform);
    //        MeasureObj.transform.position = eventData.Pointer.Result.Details.Point;
    //        line.SetActive(true);
    //    }
    //    else
    //    {
    //        if (Menu.GetComponent<MenuEvents>().measureMode() == 0)
    //        {
    //            // Clean up!
    //            line.SetActive(false);
    //            MeasureObj.SetActive(false);
    //        }

    //        // The mark.
    //        MarkObj.SetActive(true);
    //        MarkObj.transform.rotation = this.transform.rotation;
    //        MarkObj.transform.SetParent(this.transform);
    //        MarkObj.transform.position = eventData.Pointer.Result.Details.Point;
    //        Debug.Log(eventData.Pointer.Result.Details.Point);
    //    }
    //}

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
    public new void ResetRadar(bool whiten)
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
    public new void ToggleRadar(bool toggle)
    {
        this.transform.GetComponent<BoxCollider>().enabled = toggle;
        // this.transform.GetComponent<BoundsControl>().enabled = toggle;
        transform.GetChild(0).gameObject.SetActive(toggle);
        transform.GetChild(1).gameObject.SetActive(toggle);
        MarkObj.gameObject.SetActive((MarkObj.transform.parent == this.transform) && toggle);
    }

    // Toggle the line.
    public void ToggleLine(bool toggle)
    {
        if (CSVLine) CSVLine.localScale = toggle ? LineScale : new Vector3(0, 0, 0);
    }

}