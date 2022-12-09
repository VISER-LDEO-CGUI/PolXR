using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR;

public class HoverLabel : MonoBehaviour
{
    private GameObject backPlate;

    // Start is called before the first frame update
    void Start()
    {
        backPlate = this.transform.GetChild(0).gameObject;
        setEnabled(false);
    }

    public void setEnabled(bool input)
    {
        this.GetComponent<TextMeshPro>().enabled = input;
        backPlate.SetActive(input);
    }
}
