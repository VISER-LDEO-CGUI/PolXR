using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestActionMap : MonoBehaviour
{
    // Start is called before the first frame update

    public InputActionAsset inputActionSetting;

    public InputAction rightPrimaryButton;
    public InputAction rightSecondaryButton;
    public InputAction leftPrimaryButton;

    void Start()
    {
        Debug.Log(inputActionSetting);
        rightPrimaryButton = inputActionSetting.FindActionMap("XRI RightHand Interaction").FindAction("ToggleSurface");
        Debug.Log(rightPrimaryButton);

        rightSecondaryButton = inputActionSetting.FindActionMap("XRI RightHand Interaction").FindAction("ToggleBottom");
        leftPrimaryButton = inputActionSetting.FindActionMap("XRI LeftHand Interaction").FindAction("Radargram Toggle");
    }

    // Update is called once per frame
    void Update()
    {
        if (rightPrimaryButton.triggered)
        {
            Debug.Log("pressed button");
            GameObject DEM = GameObject.Find("DEMs(Clone)");
            NetworkedDEMController DEMController = DEM.GetComponent<NetworkedDEMController>();
            DEMController.toggle("MEASURES_NSIDC-0715-002");

        }
        else if (rightSecondaryButton.triggered)
        {
            GameObject DEM = GameObject.Find("DEMs(Clone)");
            NetworkedDEMController DEMController = DEM.GetComponent<NetworkedDEMController>();
            DEMController.toggle("bottom");
        }
        else if (leftPrimaryButton.triggered)
        {
            GameObject[] radargrams = GameObject.FindGameObjectsWithTag("Radargram");
            foreach (GameObject radargram in radargrams)
            {
                NetworkedRadargramController radargramController = radargram.GetComponent<NetworkedRadargramController>();
                radargramController.meshToggle();
            }
            Debug.Log("try to turn off radargram");
        }
    }
}
