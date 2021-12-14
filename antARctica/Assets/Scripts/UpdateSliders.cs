using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSliders : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform radarImage;
    public PinchSlider horizontalSlider;
    public PinchSlider verticalSlider;
    public PinchSlider rotationSlider;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Vector3 originalPosition;

    void Start()
    {
        originalScale = radarImage.localScale;
        originalRotation = radarImage.rotation.eulerAngles;
        originalPosition = radarImage.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentScale = radarImage.localScale;
        horizontalSlider.SliderValue = currentScale.x / originalScale.x - 1;
        verticalSlider.SliderValue = currentScale.y / originalScale.y - 1;
        rotationSlider.SliderValue = (float)((radarImage.rotation.eulerAngles.y - originalRotation.y) / 359.9);
    }

    public void ResetRadar()
    {
        radarImage.position = originalPosition;
        radarImage.rotation = Quaternion.Euler(originalRotation);
        radarImage.localScale = originalScale;
    }
}
