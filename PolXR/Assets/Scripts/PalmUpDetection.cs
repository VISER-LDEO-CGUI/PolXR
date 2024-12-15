using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmUpDetection : MonoBehaviour
{
    public Transform wristTransform;
    public Canvas menuCanvas;
    public float palmUpThreshold = 0.8f;

    void Update()
    {
        Vector3 handupVector = wristTransform.up;
        float dotProduct = Vector3.Dot(handupVector, Vector3.up);

        if (dotProduct > palmUpThreshold)
        {
            menuCanvas.enabled = true;
        }
        else {
            menuCanvas.enabled = false;
        }
    }
}
