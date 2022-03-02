using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SliderEvents : MonoBehaviour
{
    // Initially set to an empty object to avoid null reference.
    public Transform radarImage;

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

    // Dimension calculations
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

    void Start()
    {
        // Deactivate the menu before selection
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    // Reset the original radar image transform and re-assign the new radar image.
    public void ResetRadar(Transform newRadar)
    {
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

    // The reset button
    public void ResetButton ()
    {
        radarImage.position = originalPosition;
        radarImage.rotation = Quaternion.Euler(originalRotation);
        radarImage.localScale = originalScale;
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
