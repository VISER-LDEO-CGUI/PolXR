using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class ReturnToHome : MonoBehaviour
{
    public string[] scenePaths;
    
    readonly int HOMESCREEN_INDEX = 2;

    // The data needed for smoothing the menu movement.
    private Vector3 targetPosition;
    private Vector3 targetScale = new Vector3(1.0f, 1.0f, 1.0f);
    private bool updatePosition = true;

    // Radar Menu Toggle Buttons
    public Interactable HomeMenuButton;

    // Radar Menu Toggle Buttons
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void returnToHomeScreen()
    {
        SceneManager.LoadScene(scenePaths[HOMESCREEN_INDEX], LoadSceneMode.Single);
    }
}
