using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UpdateSliders : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform radarImage;

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
    public GameObject VerticalText;
    private TextMeshPro VerticalTMP;
    public GameObject HorizontalText;
    private TextMeshPro HorizontalTMP;
    public GameObject RotationDegreeText;
    private TextMeshPro RotationDegreeTMP;

    void Start()
    {
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
        HorizontalTMP = HorizontalText.GetComponent<TextMeshPro>(); // going to need a database for this/some spreadsheet with the values
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
            if (radarImage != null)
            {
                radarImage.position = originalPosition;
                radarImage.rotation = Quaternion.Euler(originalRotation);
                radarImage.localScale = originalScale;
            }
            radarImage = newRadar;
            originalScale = radarImage.localScale;
            originalRotation = radarImage.rotation.eulerAngles;
            originalPosition = radarImage.position;

            // Set original scale values & coefficients
            scaleX = radarImage.localScale.x;
            scaleY = radarImage.localScale.y;
            scaleZ = radarImage.localScale.z;
            vertScaleValue = 1;
            hozScaleValue = 1;

            // Set original dimension values
            OriginalHeight = scaleY * scale;
            OriginalWidth = scaleX * scale;
            VerticalTMP = VerticalText.GetComponent<TextMeshPro>(); // going to need a database for this/some spreadsheet with the values
            VerticalTMP.text = string.Format(
                "Original:   {0} m \n" +
                "Current:    {1} m \n" +
                "Strain:     {2}",
                OriginalHeight.ToString(), OriginalHeight.ToString(), 0);
            HorizontalTMP = HorizontalText.GetComponent<TextMeshPro>(); // going to need a database for this/some spreadsheet with the values
            HorizontalTMP.text = string.Format(
                "Original:   {0} m \n" +
                "Current:    {1} m \n" +
                "Strain:     {2}",
                OriginalWidth.ToString(), OriginalWidth.ToString(), 0);

            // Instantiate and set rotation
            RotationDegreeTMP = RotationDegreeText.GetComponent<TextMeshPro>();
            RotationDegreeTMP.text = radarImage.rotation.y.ToString();
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
