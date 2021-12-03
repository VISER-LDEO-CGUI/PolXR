using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UIControl : MonoBehaviour
{
    public Text txt;
    public GameObject MarkObj;
    public bool SaveFile = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //txt.text = MarkObj.transform.localPosition.ToString();
        float radarx = MarkObj.transform.localPosition.x * MarkObj.transform.parent.gameObject.transform.localScale.x * 10000;
        Debug.Log(radarx);
        //Debug.Log(MarkObj.transform.parent.gameObject.transform.localScale.x);

        // Reference https://forum.unity.com/threads/how-to-write-a-file.8864/
        if (SaveFile)
        {
            var sr = File.CreateText("temp.txt");
            sr.WriteLine(txt.text);
            sr.Close();
            SaveFile = false;
        }
    }
}
