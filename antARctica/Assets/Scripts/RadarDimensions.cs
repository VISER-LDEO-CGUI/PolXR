using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;

public class RadarDimensions : MonoBehaviour
{
    // Radar collider & scale factor
    public GameObject RadarCuboid;
    private BoxCollider CurrentCollider;
    public float scale;

    // Sliders
    public PinchSlider VertSlider;
    public PinchSlider HozSlider;
    public PinchSlider RotationSlider;

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

    void Awake()
    {
        CurrentCollider = RadarCuboid.GetComponent<BoxCollider>();

        // Set original scale values & coefficients
        scaleX = RadarCuboid.transform.localScale.x;
        scaleY = RadarCuboid.transform.localScale.y;
        scaleZ = RadarCuboid.transform.localScale.z;
        vertScaleValue = 1;
        hozScaleValue = 1;

        // Set original dimension values
        OriginalHeight = CurrentCollider.bounds.size.y * scale;
        OriginalWidth = CurrentCollider.bounds.size.x * scale;
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
        RotationDegreeTMP.text = RadarCuboid.transform.rotation.y.ToString();
    }

    void Update()
    {
        /*
        // Update slider values
        VertSlider.SliderValue = (RadarCuboid.transform.localScale.y / scaleY) - 1; // these next two only work one at a time
        HozSlider.SliderValue = (RadarCuboid.transform.localScale.x / scaleX) - 1;
        RotationSlider.SliderValue = RadarCuboid.transform.rotation.y / 180; // this doesn't work at all
        */

        // Get current dimensions of the radar image
        ScaledHeight = CurrentCollider.bounds.size.y * scale;
        ScaledWidth = CurrentCollider.bounds.size.x * scale;

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
        RotationDegreeTMP.text = string.Format("ROTATION:      {0}°", RadarCuboid.transform.localEulerAngles.y.ToString());

    }

    public void OnVerticalSliderUpdated(SliderEventData eventData)
    {
        vertScaleValue = 1 + eventData.NewValue;
        RadarCuboid.transform.localScale = new Vector3(scaleX * hozScaleValue, scaleY * vertScaleValue, scaleZ);
    }

    public void OnHorizontalSliderUpdated(SliderEventData eventData)
    {
        hozScaleValue = 1 + eventData.NewValue;
        RadarCuboid.transform.localScale = new Vector3(scaleX * hozScaleValue, scaleY * vertScaleValue, scaleZ);
    }

    public void OnRotateSliderUpdated(SliderEventData eventData)
    {
        float rotate = 180 * eventData.NewValue;
        RadarCuboid.transform.localRotation = Quaternion.Euler(0, rotate, 0);
    }

}
