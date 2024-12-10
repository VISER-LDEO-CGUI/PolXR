using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeToggle : MonoBehaviour
{
    public Toggle modeToggle;
    // Start is called before the first frame update
    void Start()
    {
        modeToggle.onValueChanged.AddListener(OnToggleChanged);

        SetMode(Mode.Snap); //default 
        modeToggle.isOn = false;
    }

    public void OnToggleChanged(bool isOn)
    {
        Debug.Log("Toggle changed. New state: " + isOn);
        if(isOn)
        {
            SetMode(Mode.Free);
        }
        else {
            SetMode(Mode.Snap);
        }
    }
    // Update is called once per frame
    void SetMode(Mode mode)
    {
        if(ModeManager.Instance != null)
        {
            ModeManager.Instance.SetMode(mode);
            Debug.Log("Mode changed to" + mode.ToString());
        }
        else 
        {
            Debug.LogWarning("ModeManager instance not set");
        }
    }
}
