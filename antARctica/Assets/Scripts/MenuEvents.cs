using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class MenuEvents : MonoBehaviour
{
    // Initially set to an empty object to avoid null reference.
    public Transform radarImage;

    // The data needed for smoothing the menu movement.
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetScale;

    // The three sliders.
    public PinchSlider horizontalSlider;
    public PinchSlider verticalSlider;
    public PinchSlider rotationSlider;

    // The initial scale, rotation and position of the radar image.
    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Vector3 originalPosition;

    // The scale for calculating the text value
    public float scale = 1000;

    // Transform.scale values
    private float scaleX;
    private float scaleY;
    private float scaleZ;

    // Scale coefficients
    private float vertScaleValue;
    private float hozScaleValue;

    // Dimension calculations.
    private float OriginalHeight;
    private float OriginalWidth;
    private float ScaledHeight;
    private float ScaledWidth;
    private float StrainHeight;
    private float StrainWidth;

    // Text objects
    public TextMeshPro Title;
    public TextMeshPro VerticalTMP;
    public TextMeshPro HorizontalTMP;
    public TextMeshPro RotationDegreeTMP;
    public TextMeshPro MarkTMP;

    // The information needed for updating the selected point coordinates.
    public GameObject MarkObj;
    public string SelectionDialog = "Assets/dialog.txt";
    private float yOrigin = 1.75f / 5.5f;

    void Start()
    {
        // Deactivate the menu before any selection happens.
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // The animation for menu.
        this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, 0.5f);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, 0.5f);
        this.transform.localScale = Vector3.Lerp(this.transform.localScale, targetScale, 0.5f);
        if (this.transform.localScale.x < 0.1f) this.gameObject.SetActive(false);

        // Update the slider value accordingly.
        Vector3 currentScale = radarImage.localScale;
        horizontalSlider.SliderValue = currentScale.x / originalScale.x - 1;
        verticalSlider.SliderValue = currentScale.y / originalScale.y - 1;
        rotationSlider.SliderValue = (float)((radarImage.rotation.eulerAngles.y - originalRotation.y) / 359.9);

        // Set original scale values & coefficients
        float updatedScaleX = radarImage.localScale.x;
        float updatedScaleY = radarImage.localScale.y;

        // Get current dimensions of the radar image
        ScaledHeight = updatedScaleY * scale;
        ScaledWidth = updatedScaleX * scale;

        // Calculate strain
        StrainHeight = Math.Abs(OriginalHeight - ScaledHeight);
        StrainWidth = Math.Abs(OriginalWidth - ScaledWidth);

        // Set scaled dimensions text
        VerticalTMP.text = string.Format(
            "Original:   {0} m \n" +
            "Current:    {1} m \n" +
            "Strain:     {2}",
            OriginalHeight.ToString(), ScaledHeight.ToString(), StrainHeight.ToString());
        
        // going to need a database for this/some spreadsheet with the values
        HorizontalTMP.text = string.Format(
            "Original:   {0} m \n" +
            "Current:    {1} m \n" +
            "Strain:     {2}",
            OriginalWidth.ToString(), ScaledWidth.ToString(), StrainWidth.ToString());

        // Set rotation text
        RotationDegreeTMP.text = string.Format("ROTATION:      {0}°", radarImage.localEulerAngles.y.ToString());

        // Update the selected point coordinates
        float maxX = MarkObj.transform.parent.gameObject.transform.localScale.x * 10000;
        float maxY = MarkObj.transform.parent.gameObject.transform.localScale.y * 100;
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

    // Reset the original radar image transform and re-assign the new radar image.
    public void ResetRadar(Transform newRadar, Vector3 newPosition)
    {
        // Adjust the new position and rotation.
        targetPosition = newPosition;
        targetRotation = Camera.main.transform.rotation;

        if (radarImage != newRadar)
        {
            // This reset may not be needed depending on the design.
            if (radarImage != null && radarImage.name != "Empty")
                ResetButton();

            // Switch to new radar and reset the values.
            radarImage = newRadar;
            originalScale = radarImage.localScale;
            originalRotation = radarImage.rotation.eulerAngles;
            originalPosition = radarImage.position;

            // Set the title of the menu to the current radar.
            Title.text = radarImage.name;

            // Set original scale values & coefficients
            scaleX = radarImage.localScale.x;
            scaleY = radarImage.localScale.y;
            scaleZ = radarImage.localScale.z;
            vertScaleValue = 1;
            hozScaleValue = 1;

            // Set original dimension values
            OriginalHeight = scaleY * scale;
            OriginalWidth = scaleX * scale;
        }
    }

    // The reset button for the radarImage transform.
    public void ResetButton ()
    {
        radarImage.position = originalPosition;
        radarImage.rotation = Quaternion.Euler(originalRotation);
        radarImage.localScale = originalScale;
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
            this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            this.gameObject.SetActive(true);
        }
    }

    // The three slider update interface.
    public void OnVerticalSliderUpdated(SliderEventData eventData)
    {
        vertScaleValue = 1 + eventData.NewValue;
        radarImage.localScale = new Vector3(scaleX * hozScaleValue, scaleY * vertScaleValue, scaleZ);
    }

    public void OnHorizontalSliderUpdated(SliderEventData eventData)
    {
        hozScaleValue = 1 + eventData.NewValue;
        radarImage.localScale = new Vector3(scaleX * hozScaleValue, scaleY * vertScaleValue, scaleZ);
    }

    public void OnRotateSliderUpdated(SliderEventData eventData)
    {
        float rotate = (float)(359.9 * eventData.NewValue);
        radarImage.localRotation = Quaternion.Euler(0, rotate, 0);
    }
}
