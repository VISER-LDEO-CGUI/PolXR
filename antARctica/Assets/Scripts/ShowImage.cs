using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShowImage : MonoBehaviour
{
    public Texture content;

    // Start is called before the first frame update
    void Start()
    {
        //content = ...
        transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", content);
        transform.GetChild(1).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", content);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
