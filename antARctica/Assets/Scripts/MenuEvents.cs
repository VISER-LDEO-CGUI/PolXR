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
    private float scaleX;
    private float scaleY;

    // The scale for calculating the text value
    public float scale = 1000;

    // Text objects
    public TextMeshPro Title;
    public TextMeshPro VerticalTMP;
    public TextMeshPro HorizontalTMP;
    public TextMeshPro RotationDegreeTMP;
    public TextMeshPro TransparencyTMP;
    public TextMeshPro MarkTMP;

    // Radar Menu Toggle Buttons
    public Interactable RadarToggle;
    public Interactable CSVPicksToggle;
    public Interactable AllRadarToggle;
    public Interactable AllCSVPicksToggle;
    public Interactable MeasurementToggle;
    public bool GetMeasureMode() { return MeasurementToggle.IsToggled; }

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
    }

    // Update is called once per frame
    void Update()
    {
        // The starting animation for menu.
        if (Vector3.Distance(targetPosition, this.transform.position) > 2) updatePosition = true;
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

            // Set rotation text
            TransparencyTMP.text = string.Format("Transparency:      {0}%", transparencySlider.SliderValue * 100);

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

            if (MarkObj.transform.parent.name != "Antarctica")
            {
                MarkTMP.text = string.Format(
                    "{0}: ({1}, {2})\n" +
                    "X: {3}, Y: {4}\n",
                    MarkObj.transform.parent.name, radarX.ToString(), radarY.ToString(), maxX.ToString(), maxY.ToString());
                if (GetMeasureMode())
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
            foreach (Transform child in RadarImagesContainer) child.GetComponent<RadarEvents>().ResetRadar();
            MainCSVToggling();
        }
        else
        {
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

        if (File.Exists(SelectionDialog))
        {
            List<string> tempList = new List<string> { MarkTMP.text };
            File.AppendAllLines(SelectionDialog, tempList);
        }
        else
        {
            var sr = File.CreateText(SelectionDialog);
            sr.WriteLine(MarkTMP.text);
            sr.Close();
        }

        ParticleSystem CSVLine = radarImage.Find("Line").GetComponent<ParticleSystem>();
        var main = CSVLine.main;
        int CSVLength = main.maxParticles + 1;
        ParticleSystem.Particle[] CSVPoints = new ParticleSystem.Particle[CSVLength];
        CSVLine.GetParticles(CSVPoints);

        // Transform the position into particle coordinates.
        Vector3 newPos = MarkObj.transform.TransformPoint(Vector3.zero) - CSVLine.transform.TransformPoint(Vector3.zero);
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
        scaleY = 0.5f + eventData.NewValue;
        if (radarImage) radarImage.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
    }

    public void OnHorizontalSliderUpdated(SliderEventData eventData)
    {
        scaleX = 0.5f + eventData.NewValue;
        if (radarImage) radarImage.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
    }

    public void OnRotateSliderUpdated(SliderEventData eventData)
    {
        float rotate = (float)(360.0f * eventData.NewValue);
        if (radarImage) radarImage.localRotation = Quaternion.Euler(0, rotate, 0);
    }

    public void OnTransparencySliderUpdated(SliderEventData eventData)
    {
        if (radarImage) radarImage.GetComponent<RadarEvents>().SetAlpha(1 - eventData.NewValue);
    }

    // Main Menu Vertical Exaggeration Slider
    public void OnVerticalExaggerationSliderUpdated(SliderEventData eventData)
    {
        foreach (Transform child in Dems) child.localScale = new Vector3(1, 0.5f + eventData.NewValue, 1);
    }

    // Main Menu Toggling CSV and radar images.
    public void MainCSVToggling()
    {
        Vector3 newScale = AllCSVPicksToggle.IsToggled ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
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
        bool originalState = Antarctica.GetComponent<BoxCollider>().enabled;
        Antarctica.GetComponent<BoxCollider>().enabled = !originalState;
        Antarctica.GetComponent<BoundsControl>().enabled = !originalState;
    }

    // Synchronize the sliders.
    public void ConstraintSlider(float x, float y)
    {
        horizontalSlider.SliderValue = x;
        verticalSlider.SliderValue = y;
    }

    // Move the user to somewhere near the selected radar.
    public void TeleportationButton()
    {
        if (MarkObj.transform.parent.name != "Antarctica" && MarkObj.activeSelf)
        {
            Vector3 tlpOffset = (Camera.main.transform.position - radarImage.transform.position).normalized;
            MixedRealityPlayspace.Transform.Translate(radarImage.transform.position + tlpOffset);
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
        if (this.gameObject.activeSelf) CloseButton(true);
        else CloseButton(false);
    }

    // The voice command version for the toggles.
    public void ToggleVoice(string keyword)
    {
        if (keyword == "measure") MeasurementToggle.IsToggled = !MeasurementToggle.IsToggled;
        else if (keyword == "box") BoundingBoxToggle();
    }
}