using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class MenuEvents : MonoBehaviour
{
    // The current menu type, true stands for main menu.
    private bool MainMenu;
    public Transform SubMenuRadar;
    public Transform SubMenuMain;

    // Initially set to an empty object to avoid null reference.
    private Transform radarImage = null;
    public Transform RadarImagesContainer;
    public Transform CSVPicksContainer;
    public Transform Dems;
    public Transform Antarctica;

    // The data needed for smoothing the menu movement.
    private Vector3 targetPosition;
    private Vector3 targetScale = new Vector3(1.0f, 1.0f, 1.0f);
    private bool updatePosition = true;

    // The sliders.
    public PinchSlider horizontalSlider;
    public PinchSlider verticalSlider;
    public PinchSlider rotationSlider;
    public PinchSlider transparencySlider;
    public PinchSlider verticalExaggerationSlider;

    // The initial scale, rotation and position of the radar image.
    private Vector3 originalScale;
    private float scaleX = 1.0f;
    private float scaleY = 1.0f;

    // The scale for calculating the text value
    public float scale = 1000;

    // Text objects
    public TextMeshPro Title;
    public TextMeshPro VerticalTMP;
    public TextMeshPro HorizontalTMP;
    public TextMeshPro RotationDegreeTMP;
    public TextMeshPro TransparencyTMP;
    public TextMeshPro MarkTMP;
    public TextMeshPro MeasureModeText;

    // Radar Menu Toggle Buttons
    public Interactable RadarToggle;
    public Interactable CSVPicksToggle;
    public Interactable AllRadarToggle;
    public Interactable AllCSVPicksToggle;
    public Interactable MeasurementToggle;
    public Interactable SecondMeasurementToggle;
    public Interactable SurfaceToggle;
    public Interactable BedToggle;
    public Interactable BoxToggle;

    // The information needed for updating the selected point coordinates.
    public GameObject MarkObj;
    public Color MarkColor;
    public GameObject MeasureObj;
    public GameObject MeasureLine;
    public string SelectionDialog = "Assets/dialog.txt";
    private float yOrigin = 1.75f / 5.5f;

    // The minimap plate.
    public GameObject Minimap;

    void Start()
    {
        // Deactivate the radar menu before any selection happens; deactivate the bounding box.
        HomeButton(true);
        BoundingBoxToggle();
        MeasureLine.SetActive(false);
        Minimap.GetComponent<BoxCollider>().enabled = false;
        MarkObj.transform.parent = Antarctica.transform;
        MarkObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // The starting animation for menu.
        if (Vector3.Distance(targetPosition, this.transform.position) > 1) updatePosition = true;
        else if (Vector3.Distance(targetPosition, this.transform.position) < 0.01f) updatePosition = false;
        if (updatePosition) this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, 0.5f);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Camera.main.transform.rotation, 0.02f);
        this.transform.localScale = Vector3.Lerp(this.transform.localScale, targetScale, 0.5f);
        if (this.transform.localScale.x < 0.1f) this.gameObject.SetActive(false);

        if (!MainMenu)
        {
            // Update the rotation slider value accordingly.
            float rounded_angle = (float)(radarImage.localRotation.eulerAngles.y / 360.0f);
            rounded_angle = rounded_angle >= 0 ? rounded_angle : rounded_angle + 1.0f;
            if (Mathf.Abs(rotationSlider.SliderValue - rounded_angle) > 0.01f)
                rotationSlider.SliderValue = rounded_angle;

            // Set scaled dimensions text
            VerticalTMP.text = string.Format(
                "Original:   {0} m \n" +
                "Current:    {1} m \n" +
                "Strain:     {2}",
                (originalScale.y * scale).ToString(),
                (radarImage.localScale.y * scale).ToString(),
                (Math.Abs(originalScale.y - radarImage.localScale.y) * scale).ToString());

            // going to need a database for this/some spreadsheet with the values
            HorizontalTMP.text = string.Format(
                "Original:   {0} m \n" +
                "Current:    {1} m \n" +
                "Strain:     {2}",
                (originalScale.x * scale).ToString(),
                (radarImage.localScale.x * scale).ToString(),
                (Math.Abs(originalScale.x - radarImage.localScale.x) * scale).ToString());

            // Set rotation text
            RotationDegreeTMP.text = string.Format("ROTATION:      {0}°", radarImage.localEulerAngles.y.ToString());

            // Set transparency text
            TransparencyTMP.text = string.Format("Transparency:      {0}%", Mathf.Round(transparencySlider.SliderValue * 4) * 25);

            // Update the selected point coordinates
            float maxX = radarImage.localScale.x * scale;
            float maxY = radarImage.localScale.y * scale;
            float radarX = (MarkObj.transform.localPosition.x + 0.5f) * maxX;
            float radarY = (MarkObj.transform.localPosition.y - yOrigin) * maxY;
            Vector2 measure;
            if (MeasureObj.activeSelf == false) measure = new Vector2(0, 0);
            else
                measure = new Vector2((MeasureObj.transform.localPosition.x - MarkObj.transform.localPosition.x) * maxX,
                (MeasureObj.transform.localPosition.y - MarkObj.transform.localPosition.y) * maxY);

            if (MarkObj.activeSelf)
            {
                MarkTMP.text = string.Format(
                    "{0}: ({1}, {2})\n" +
                    "X: {3}, Y: {4}\n",
                    MarkObj.transform.parent.name, radarX.ToString(), radarY.ToString(), maxX.ToString(), maxY.ToString());
                if (measureMode() != 0)
                    MarkTMP.text += string.Format("Distance: {0}m", Vector2.Distance(measure, new Vector2(0, 0)));
            }
                
            else
                MarkTMP.text = "No selected points.";
        }
    }

    // Reset the original radar image transform and re-assign the new radar image.
    public void ResetRadarSelected(Transform newRadar, Vector3 newPosition, float newAlpha)
    {
        targetPosition = newPosition;

        if (radarImage != newRadar)
        {
            // Switch to new radar and reset the values.
            radarImage = newRadar;
            originalScale = radarImage.GetComponent<RadarEvents>().GetScale();

            // Set the title of the menu to the current radar.
            Title.text = radarImage.name;
        }

        MainMenu = false;
        RadarToggle.IsToggled = true;
        SubMenuRadar.gameObject.SetActive(true);
        SubMenuMain.gameObject.SetActive(false);
    }

    // The reset button for the radarImage transform.
    public void ResetButton()
    {
        if (MainMenu)
        {
            AllCSVPicksToggle.IsToggled = true;
            AllRadarToggle.IsToggled = true;
            MarkObj.transform.parent = Antarctica.transform;
            MarkObj.SetActive(false);
            foreach (Transform child in RadarImagesContainer) child.GetComponent<RadarEvents>().ResetRadar();
            MainCSVToggling();
        }
        // The snap function.
        else if (measureMode() != 0)
        {
            if (MeasureObj.activeSelf && MeasureObj.transform.parent != MarkObj.transform.parent)
            {
                MeasureObj.transform.parent.rotation = MarkObj.transform.parent.rotation;

                // Compute the offset and merge the measuring image to the marked radar image.
                Vector3 snapOffset = (MeasureObj.transform.position - MarkObj.transform.position);

                // If they are too close and user wants to, reset the image gap.
                if (snapOffset.magnitude < 0.001f) snapOffset = MeasureObj.transform.forward * 0.1f;

                MeasureObj.transform.parent.position -= snapOffset;

                // Set transparency for better comparisons: only for images not so transparent.
                MeasureObj.transform.parent.gameObject.GetComponent<RadarEvents>().SetAlpha(0.5f, true);
                MarkObj.transform.parent.gameObject.GetComponent<RadarEvents>().SetAlpha(0.5f, true);
                transparencySlider.SliderValue = 0.5f;
            }
        }
        else
        {
            // Reset radar menu and radar attributes.
            RadarToggle.IsToggled = true;
            CSVPicksToggle.IsToggled = true;
            radarImage.GetComponent<RadarEvents>().ResetRadar();
            radarImage.GetComponent<RadarEvents>().ToggleLine(true);
        }
    }

    // The write button for writting the coordinates into a file.
    // Reference https://forum.unity.com/threads/how-to-write-a-file.8864/
    // Be aware of the file path issue! And try to keep a history...
    public void WriteButton()
    {
        RadarToggle.IsToggled = true;
        RadarToggling();
        CSVPicksToggle.IsToggled = true;
        CSVToggling();

        /*if (File.Exists(SelectionDialog))
        {
            List<string> tempList = new List<string> { MarkTMP.text };
            File.AppendAllLines(SelectionDialog, tempList);
        }
        else
        {
            var sr = File.CreateText(SelectionDialog);
            sr.WriteLine(MarkTMP.text);
            sr.Close();
        }*/

        ParticleSystem CSVLine = radarImage.Find("Line").GetComponent<ParticleSystem>();
        var main = CSVLine.main;
        int CSVLength = main.maxParticles + 1;
        ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[CSVLength];
        CSVLine.GetParticles(CSVPoints);

        // Transform the position into particle coordinates.
        Vector3 newPos = MarkObj.transform.position - CSVLine.transform.position;
        newPos = Quaternion.Euler(0, -radarImage.transform.localEulerAngles.y, 0) * newPos;
        newPos.x /= CSVLine.transform.lossyScale.x;
        newPos.y /= CSVLine.transform.lossyScale.y;
        newPos.z /= CSVLine.transform.lossyScale.z;

        // Set the particle format.
        CSVPoints[CSVLength - 1] = CSVPoints[0];
        CSVPoints[CSVLength - 1].position = newPos;
        CSVPoints[CSVLength - 1].startColor = MarkColor;

        // Emit and set the new particle.
        main.maxParticles += 1;
        CSVLine.Emit(1);
        CSVLine.SetParticles(CSVPoints, CSVLength);
    }

    // The close button, make the menu disappear and deactivated.
    public void CloseButton(bool shutDown)
    {
        if (shutDown) targetScale = new Vector3(0.0f, 0.0f, 0.0f);
        else
        {
            targetScale = new Vector3(1.0f, 1.0f, 1.0f);
            targetPosition = Camera.main.transform.position + Camera.main.transform.forward;
            if (this.transform.localScale.x < 0.1f) this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            this.gameObject.SetActive(true);
        }
    }

    // Switch between different sub menus.
    public void HomeButton(bool home)
    {
        MainMenu = home;
        Title.text = home? "AntARctica" : radarImage.name;
        SubMenuRadar.gameObject.SetActive(!home);
        SubMenuMain.gameObject.SetActive(home);
    }

    // The four slider update interface.
    public void OnVerticalSliderUpdated(SliderEventData eventData)
    {
        if (radarImage && verticalSlider.gameObject.tag == "Active")
            radarImage.localScale = new Vector3(radarImage.localScale.x, originalScale.y * scaleY * (0.5f + eventData.NewValue), originalScale.z);
    }

    public void OnHorizontalSliderUpdated(SliderEventData eventData)
    {
        if (radarImage && horizontalSlider.gameObject.tag == "Active")
            radarImage.localScale = new Vector3(originalScale.x * scaleX * (0.5f + eventData.NewValue), radarImage.localScale.y, originalScale.z);
    }

    public void OnRotateSliderUpdated(SliderEventData eventData)
    {
        float rotate = (float)(360.0f * eventData.NewValue);
        if (radarImage) radarImage.localRotation = Quaternion.Euler(0, rotate, 0);
    }

    public void OnTransparencySliderUpdated(SliderEventData eventData)
    {
        //Round the result to nearest levels.
        transparencySlider.SliderValue = Mathf.Round(eventData.NewValue * 4) / 4;
        if (radarImage) radarImage.GetComponent<RadarEvents>().SetAlpha(1 - eventData.NewValue);
    }

    // Main Menu Vertical Exaggeration Slider
    public void OnVerticalExaggerationSliderUpdated(SliderEventData eventData)
    {
        foreach (Transform child in Dems) child.localScale = new Vector3(1, 0.1f + (4.9f * eventData.NewValue), 1);
    }

    // Main Menu Toggling CSV and radar images.
    public void MainCSVToggling()
    {
        Vector3 newScale = AllCSVPicksToggle.IsToggled ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
        CSVPicksToggle.IsToggled = AllCSVPicksToggle.IsToggled;
        foreach (Transform child in CSVPicksContainer) child.localScale = newScale;
        foreach (Transform child in RadarImagesContainer)
            child.GetComponent<RadarEvents>().ToggleLine(AllCSVPicksToggle.IsToggled);
    }

    public void MainRadarToggling()
    {
        foreach (Transform child in RadarImagesContainer)
            child.GetComponent<RadarEvents>().ToggleRadar(AllRadarToggle.IsToggled);
    }

    // Single radar toggling.
    public void CSVToggling() { radarImage.GetComponent<RadarEvents>().ToggleLine(CSVPicksToggle.IsToggled); }
    public void RadarToggling() { radarImage.GetComponent<RadarEvents>().ToggleRadar(RadarToggle.IsToggled); }

    // Find the dem according to name.
    public void DemToggle(string name)
    {
        GameObject targetDem = Dems.Find(name).gameObject;
        targetDem.SetActive(!targetDem.activeSelf);
    }

    // Switch between two states of the bounding box.
    public void BoundingBoxToggle()
    {
        if (!SurfaceToggle.IsToggled)
        {
            DemToggle("Bedmap2_surface_RIS");
            SurfaceToggle.IsToggled = true;
        }

        if (!BedToggle.IsToggled)
        {
            DemToggle("Bedmap2_bed");
            BedToggle.IsToggled = true;
        }
        bool originalState = Antarctica.GetComponent<BoxCollider>().enabled;
        BoxToggle.IsToggled = !originalState;
        Antarctica.GetComponent<BoxCollider>().enabled = !originalState;
        Antarctica.GetComponent<BoundsControl>().enabled = !originalState;
    }

    // Synchronize the measurement toggle function.
    // 0 for not turned on, 1 for measure object (end), 2 for mark object (start).
    public int measureMode(bool voiceSync = false)
    {
        if (voiceSync)
        {
            SecondMeasurementToggle.IsToggled = MeasurementToggle.IsToggled && !SecondMeasurementToggle.IsToggled;
            MeasurementToggle.IsToggled = !(MeasurementToggle.IsToggled && !SecondMeasurementToggle.IsToggled);
        }

        if (!MeasurementToggle.IsToggled)
        {
            SecondMeasurementToggle.IsToggled = false;
            SecondMeasurementToggle.gameObject.SetActive(false);
            MeasureModeText.text = "MEASUREMENT MODE";
            return 0;
        }
        else
        {
            SecondMeasurementToggle.gameObject.SetActive(true);
            MeasureModeText.text = "MEASUREMENT MODE\nCHANGE START";
            return SecondMeasurementToggle.IsToggled ? 2 : 1;
        }
    }

    // Synchronize the sliders.
    public void syncScaleSlider()
    {
        if (radarImage)
        {
            scaleX = radarImage.localScale.x / originalScale.x;
            scaleY = radarImage.localScale.y / originalScale.y;
        }
    }

    // Move the user to somewhere near the selected radar.
    public void TeleportationButton()
    {
        if (MarkObj.activeSelf && (Camera.main.transform.position - MarkObj.transform.position).magnitude > 1)
        {
            Vector3 tlpOffset = (Camera.main.transform.position - MarkObj.transform.position).normalized;
            MixedRealityPlayspace.Transform.Translate(-tlpOffset);
        }
    }

    // Turn on/off the minimap.
    public void MinimapButton()
    {
        Minimap.GetComponent<BoxCollider>().enabled = !Minimap.GetComponent<BoxCollider>().enabled;
        Minimap.transform.localPosition = new Vector3(0.04f, -0.03f, 0);
    }

    // The voice command version for the close button.
    public void MenuVoice()
    {
        if (!this.gameObject.activeSelf) CloseButton(false);
        else if (!MainMenu) HomeButton(true);
        else CloseButton(true);
    }

    // The voice command version for the toggles.
    public void ToggleVoice(string keyword)
    {
        if (keyword == "measure") measureMode(true);
        else if (keyword == "box") BoundingBoxToggle();
        else if (keyword == "model")
        {
            DemToggle("Bedmap2_surface_RIS");
            DemToggle("Bedmap2_bed");
            SurfaceToggle.IsToggled = !SurfaceToggle.IsToggled;
            BedToggle.IsToggled = !BedToggle.IsToggled;
        }
        else if (keyword == "line")
        {
            if (MainMenu)
            {
                AllCSVPicksToggle.IsToggled = !AllCSVPicksToggle.IsToggled;
                MainCSVToggling();
            }
            else
            {
                CSVPicksToggle.IsToggled = !CSVPicksToggle.IsToggled;
                CSVToggling();
            }
        }
        else if (keyword == "image")
        {
            if (MainMenu)
            {
                AllRadarToggle.IsToggled = !AllRadarToggle.IsToggled;
                MainRadarToggling();
            }
            else
            {
                if (transparencySlider.SliderValue < 0.5) transparencySlider.SliderValue = 0.5f;
                else
                {
                    RadarToggle.IsToggled = !RadarToggle.IsToggled;
                    RadarToggling();
                    if (RadarToggle.IsToggled) transparencySlider.SliderValue = 0;
                }
            }
        }
    }
}
