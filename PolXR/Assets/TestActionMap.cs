using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestActionMap : MonoBehaviour
{
    // Start is called before the first frame update

    public InputActionAsset a;

    public InputAction primaryButton;

    void Start()
    {
        Debug.Log(a);
        primaryButton = a.FindActionMap("XRI RightHand Interaction").FindAction("Toggle");
        Debug.Log(primaryButton);
    }

    // Update is called once per frame
    void Update()
    {
        if (primaryButton.triggered)
        {
            Debug.Log("pressed button");
        }
    }
}
