using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkObj : MonoBehaviour
{
    private Vector3 Original_Scale;

    // Start is called before the first frame update
    void Start()
    {
        Original_Scale = this.transform.lossyScale;
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 Global_Scale = this.transform.parent.transform.localScale;
        this.transform.localScale = new Vector3(Original_Scale.x / Global_Scale.x, Original_Scale.y / Global_Scale.y, Original_Scale.z / Global_Scale.z);
    }
}
