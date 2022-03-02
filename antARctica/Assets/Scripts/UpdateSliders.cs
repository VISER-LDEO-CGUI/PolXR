using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSliders : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform radarImage;

    public PinchSlider horizontalSlider;
    public PinchSlider verticalSlider;
    public PinchSlider rotationSlider;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Vector3 originalPosition;

    // The scale of the self object.
    private Vector3 Original_Scale;

    void Start()
    {
        // Record the starting scale.
        Original_Scale = this.transform.lossyScale;

        radarImage = this.transform.parent;
        StartRadar();
    }

    // Update is called once per frame
    void Update()
    {
        if (radarImage != this.transform.parent && this.transform.parent.name != "Antarctica")
        {
            ResetRadar();
            radarImage = this.transform.parent;
            StartRadar();
        }
        if (radarImage.name != "Antarctica")
        {
            Vector3 currentScale = radarImage.localScale;
            horizontalSlider.SliderValue = currentScale.x / originalScale.x - 1;
            verticalSlider.SliderValue = currentScale.y / originalScale.y - 1;
            rotationSlider.SliderValue = (float)((radarImage.rotation.eulerAngles.y - originalRotation.y) / 359.9);
        }

        // Adjust the scale according to new parent.
        Vector3 Global_Scale = this.transform.parent.transform.localScale;
        this.transform.localScale = new Vector3(Original_Scale.x / Global_Scale.x, Original_Scale.y / Global_Scale.y, Original_Scale.z / Global_Scale.z);
        // Compute and restore the global position and rotation here
        // ...
    }

    public void StartRadar()
    {
        originalScale = radarImage.localScale;
        originalRotation = radarImage.rotation.eulerAngles;
        originalPosition = radarImage.position;
    }

    public void ResetRadar()
    {
        radarImage.position = originalPosition;
        radarImage.rotation = Quaternion.Euler(originalRotation);
        radarImage.localScale = originalScale;
    }

    public void OnVerticalSliderUpdated(SliderEventData eventData)
    {
        if (radarImage.name != "Antarctica")
        {
            radarImage.GetComponent<RadarDimensions>().OnVerticalSliderUpdated(eventData.NewValue);
        }
    }

    public void OnHorizontalSliderUpdated(SliderEventData eventData)
    {
        if (radarImage.name != "Antarctica")
        {
            //radarImage.GetComponent<RadarDimensions>().OnHorizontalSliderUpdated(eventData.NewValue);
            radarImage.GetComponent<RadarDimensions>().OnVerticalSliderUpdated(eventData.NewValue);
        }
    }

    public void OnRotateSliderUpdated(SliderEventData eventData)
    {
        if (radarImage.name != "Antarctica")
        {
            radarImage.GetComponent<RadarDimensions>().OnRotateSliderUpdated(eventData.NewValue);
        }
    }
}
