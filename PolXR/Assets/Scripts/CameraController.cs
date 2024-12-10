using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public DataLoader dataLoader;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = dataLoader.GetDEMCentroid();
    }
}
