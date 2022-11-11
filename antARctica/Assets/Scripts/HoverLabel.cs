using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR;

public class HoverLabel : MonoBehaviour
{
    public bool controller = false;
    public float distance = 1;

    private bool findController = true;
    private InputDevice handDevice;
    private GameObject backPlate;

    // Start is called before the first frame update
    void Start()
    {
        backPlate = this.transform.GetChild(0).gameObject;
        setEnabled(false);

        // Try to locate the controllers.
        handDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (handDevice == null) handDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        findController = (handDevice == null);
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray;
        RaycastHit hit;

        // The raycast test for controller.
        if (findController && controller)
        {
            Vector3 handPosition;
            Quaternion handRotation;
            handDevice.TryGetFeatureValue(CommonUsages.devicePosition, out handPosition);
            handDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out handRotation);
            ray = new Ray(handPosition, handRotation * Vector3.forward);
        }
        // Generate a ray from camera.
        else ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // Test whether it collides with the label's parent.
        if (Physics.Raycast(ray, out hit, distance) && hit.transform == this.transform.parent)
            setEnabled(true);
        else setEnabled(false);
    }

    private void setEnabled(bool input)
    {
        this.GetComponent<TextMeshPro>().enabled = input;
        backPlate.SetActive(input);
    }
}
