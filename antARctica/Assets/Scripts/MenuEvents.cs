using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
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
    public Transform SurfaceDEM;
    public Transform BaseDEM;
    public Transform Antarctica;

    // The data needed for smoothing the menu movement.
    private Vector3 targetPosition;
    private Vector3 targetScale = new Vector3(1.0f, 1.0f, 1.0f);
    private bool updatePosition = false;

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
    public Interactable SurfaceDEMToggle;
    public Interactable BaseDEMToggle;
    public Interactable BoundingBoxToggle;

    // The information needed for updating the selected point coordinates.
    public GameObject MarkObj;
    public string SelectionDialog = "Assets/dialog.txt";
    private float yOrigin = 1.75f / 5.5f;

    void Start()
    {
        // Deactivate the radar menu before any selection happens.
        HomeButton(true);
    }

    // Update is called once per frame
    void Update()
    {
        // The starting animation for menu.
        if (Vector3.Distance(targetPosition, this.transform.position) > 2) updatePosition = true;
        else if (Vector3.Distance(targetPosition, this.transform.position) < 0.01f) updatePosition = false;
        if (updatePosition) this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, 0.5f);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Camera.main.transform.rotation, 0.01f);
        this.transform.localScale = Vector3.Lerp(this.transform.localScale, targetScale, 0.5f);
        if (this.transform.localScale.x < 0.1f) this.gameObject.SetActive(false);

        if (MainMenu)
        {
            // Main Menu Toggling Objects.
            SurfaceDEM.gameObject.SetActive(SurfaceDEMToggle.IsToggled);
            BaseDEM.gameObject.SetActive(BaseDEMToggle.IsToggled);
            MainToggling(AllCSVPicksToggle.IsToggled, AllRadarToggle.IsToggled);
            Antarctica.GetComponent<BoxCollider>().enabled = BoundingBoxToggle.IsToggled;
            Antarctica.GetComponent<BoundsControl>().enabled = BoundingBoxToggle.IsToggled;
        }
        else
        {
            // Radar Menu Toggling Objects
            radarImage.GetComponent<RadarEvents>().ToggleRadar(RadarToggle.IsToggled);
            radarImage.GetComponent<RadarEvents>().ToggleLine(CSVPicksToggle.IsToggled);

            // Update the rotation slider value accordingly.
            float rounded_angle = (float)(radarImage.rotation.eulerAngles.y / 359.9);
            rotationSlider.SliderValue = rounded_angle >= 0 ? rounded_angle : rounded_angle + 360.0f;

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
            float maxX = radarImage.localScale.x * 10000;
            float maxY = radarImage.localScale.y * 100;
            float radarX = (MarkObj.transform.localPosition.x + 0.5f) * maxX;
            float radarY = (MarkObj.transform.localPosition.y - yOrigin) * maxY;

            if (MarkObj.transform.parent.name != "Antarctica")
                MarkTMP.text = string.Format(
                    "{0}: ({1}, {2})\n" +
                    "X: {3}, Y: {4}",
                    MarkObj.transform.parent.name, radarX.ToString(), radarY.ToString(), maxX.ToString(), maxY.ToString());
            else
                MarkTMP.text = "No selected points.";
        }
    }

    // Reset the original radar image transform and re-assign the new radar image.
    public void ResetRadar(Transform newRadar, Vector3 newPosition, float newAlpha)
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
            foreach (Transform child in RadarImagesContainer) child.GetComponent<RadarEvents>().ResetRadar();
            CSVPicksToggle.IsToggled = true;
        }
        else
        {
            radarImage.GetComponent<RadarEvents>().ResetRadar();
            radarImage.GetComponent<RadarEvents>().ToggleLine(true);
        }
    }

    // The write button for writting the coordinates into a file.
    // Reference https://forum.unity.com/threads/how-to-write-a-file.8864/
    // Be aware of the file path issue! And try to keep a history...
    public void WriteButton()
    {
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
    }

    // The close button, make the menu disappear and deactivated.
    public void CloseButton(bool shutDown)
    {
        if (shutDown) targetScale = new Vector3(0.0f, 0.0f, 0.0f);
        else
        {
            targetScale = new Vector3(1.0f, 1.0f, 1.0f);
            if (this.transform.localScale.x < 0.1f) this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            this.gameObject.SetActive(true);
        }
    }

    // Switch between different sub menus.
    public void HomeButton(bool home)
    {
        MainMenu = home;
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
        float rotate = (float)(359.9 * eventData.NewValue);
        if (radarImage) radarImage.localRotation = Quaternion.Euler(0, rotate, 0);
    }

    public void OnTransparencySliderUpdated(SliderEventData eventData)
    {
        if (radarImage) radarImage.GetComponent<RadarEvents>().SetAlpha(1 - eventData.NewValue);
    }

    // Main Menu Vertical Exaggeration Slider
    public void OnVerticalExaggerationSliderUpdated(SliderEventData eventData)
    {
        SurfaceDEM.localScale = new Vector3(1, 0.5f + eventData.NewValue, 1);
        BaseDEM.localScale = new Vector3(1, 0.5f + eventData.NewValue, 1);
    }

    // Main Menu Toggling CSV and radar images.
    public void MainToggling(bool CSVInput, bool RadarInput)
    {
        Vector3 newScale = CSVInput ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
        foreach (Transform child in CSVPicksContainer) child.localScale = newScale;
        foreach (Transform child in RadarImagesContainer)
        {
            child.GetComponent<RadarEvents>().ToggleRadar(RadarInput);
            child.GetComponent<RadarEvents>().ToggleLine(CSVInput);
        }
    }

    public void ConstraintSlider(float x, float y)
    {
        horizontalSlider.SliderValue = x;
        verticalSlider.SliderValue = y;
    }
}