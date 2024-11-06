using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeManager : MonoBehaviour
{
    public static ModeManager Instance { get; private set; }
    public Mode currentMode { get; private set; } = Mode.Snap;
    private List<Radargram> selectedRadargrams = new List<Radargram>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMode(Mode mode)
    {
        currentMode = mode;
        UpdateRadargramView();
    }

    private void UpdateRadargramView()
    {
        foreach (var radargram in FindObjectsOfType<Radargram>())  
        {
            radargram.ApplyModeBehavior(currentMode); 
        }
    }

    public void SwitchMode()
    {
        // toggle between snap and free modes
        currentMode = (currentMode == Mode.Snap) ? Mode.Free : Mode.Snap;
        UpdateRadargramView();
    }

    public void AddToSelection(Radargram radargram)
    {
        if (!selectedRadargrams.Contains(radargram))
        {
            selectedRadargrams.Add(radargram);
            radargram.Highlight(true);
            UpdateRadargramView();  
        }
    }

    public void RemoveFromSelection(Radargram radargram)
    {
        if (selectedRadargrams.Contains(radargram))
        {
            selectedRadargrams.Remove(radargram);
            radargram.Highlight(false);
            UpdateRadargramView();
        }
    }

    public void LoadStudyScene()
    {
        SceneManager.LoadScene("StudyScene");
    }
}
