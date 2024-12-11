//using Microsoft.MixedReality.Toolkit;
//using Microsoft.MixedReality.Toolkit.UI;
//using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class HomeMenuEvents : MonoBehaviour
{
    public string[] scenePaths;
    // The data needed for smoothing the menu movement.
    private Vector3 targetPosition;
    private Vector3 targetScale = new Vector3(1.0f, 1.0f, 1.0f);
    private bool updatePosition = true;

    // Radar Menu Toggle Buttons
    //public Interactable AntarcticaButton;
    //public Interactable GreenlandButton;

    // Indices for scenes
    readonly int ANTARCTICA_INDEX = 0;
    readonly int GREENLAND_INDEX = 1;

    void Start()
    {
        // Deactivate the radar menu before any selection happens; deactivate the bounding box.
    }

    // Update is called once per frame
    void Update()
    {
        // The starting animation for menu.

        // if (Vector3.Distance(targetPosition, this.transform.position) > 1)
        //     updatePosition = true;
        // else
        //     if (Vector3.Distance(targetPosition, this.transform.position) < 0.01f)
        //     updatePosition = false;

        // if (updatePosition)
        //     this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, 0.5f);

        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Camera.main.transform.rotation, 0.02f);
        this.transform.localScale = Vector3.Lerp(this.transform.localScale, targetScale, 0.5f);

        if (this.transform.localScale.x < 0.1f)
            this.gameObject.SetActive(false);
    }

    public void changeAntarctica()
    {
        SceneManager.LoadScene(scenePaths[ANTARCTICA_INDEX], LoadSceneMode.Single);
    }

    public void changeGreenland()
    {
        SceneManager.LoadScene(scenePaths[GREENLAND_INDEX], LoadSceneMode.Single);
    }
}
