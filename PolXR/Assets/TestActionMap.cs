using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestActionMap : MonoBehaviour
{
    // Start is called before the first frame update

    public InputActionAsset inputActionSetting;

    public InputAction primaryButton;
    public InputAction secondaryButton;

    void Start()
    {
        Debug.Log(inputActionSetting);
        primaryButton = inputActionSetting.FindActionMap("XRI RightHand Interaction").FindAction("ToggleSurface");
        Debug.Log(primaryButton);

        secondaryButton = inputActionSetting.FindActionMap("XRI RightHand Interaction").FindAction("ToggleBottom");
    }

    // Update is called once per frame
    void Update()
    {
        if (primaryButton.triggered)
        {
            Debug.Log("pressed button");
            GameObject DEM = GameObject.Find("DEMs(Clone)");
            NetworkedDEMController DEMController = DEM.GetComponent<NetworkedDEMController>();
            DEMController.toggle("MEASURES_NSIDC-0715-002");

        }
        else if (secondaryButton.triggered)
        {
            GameObject DEM = GameObject.Find("DEMs(Clone)");
            NetworkedDEMController DEMController = DEM.GetComponent<NetworkedDEMController>();
            DEMController.toggle("bottom");
        }
    }
}
