using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShowImage : MonoBehaviour
{
    // The file root under the "Resources" folder.
    public string fileRoot = "Radar Images/";

    // Start is called before the first frame update
    void Start()
    {
        // Get and set the texture of the radar image object.
        if (System.IO.File.Exists("Assets/Resources/" + fileRoot + this.transform.parent.name + ".png"))
        {
            Texture content = Resources.Load<Texture2D>(fileRoot + this.transform.parent.name);
            transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", content);
            transform.GetChild(1).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", content);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
