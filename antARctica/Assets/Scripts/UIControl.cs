using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UIControl : MonoBehaviour
{
    // This script is only made for debugging and demo, need to be merged into menu or other UI elements in the future!
    public Text txt;
    public GameObject MarkObj;
    public bool SaveFile = false;
    private float yOrigin;

    // Start is called before the first frame update
    void Start()
    {
        yOrigin = 1.75f / 5.5f;
    }

    // Update is called once per frame
    void Update()
    {
        float maxX = MarkObj.transform.parent.gameObject.transform.localScale.x * 10000;
        float maxY = MarkObj.transform.parent.gameObject.transform.localScale.y * 100;
        float radarX = (MarkObj.transform.localPosition.x + 0.5f) * maxX;
        float radarY = (MarkObj.transform.localPosition.y - yOrigin) * maxY;
        string newText = MarkObj.transform.parent.name + ": (" + radarX.ToString() + ", " + radarY.ToString() + ")\nX: " + maxX.ToString() + ", Y: " + maxY.ToString();
        if (MarkObj.transform.parent.name != "Antarctica")
            txt.text = newText;
        else
            newText = "No selected points.";

        // Reference https://forum.unity.com/threads/how-to-write-a-file.8864/
        if (SaveFile)
        {
            if (File.Exists("Assets/temp.txt"))
            {
                List<string> tempList = new List<string> { newText };
                File.AppendAllLines("Assets/temp.txt", tempList);
            }
            else
            {
                var sr = File.CreateText("Assets/temp.txt");
                sr.WriteLine(txt.text);
                sr.Close();
            }
            SaveFile = false;
        }
    }

    public void WriteCoordinates()
    {
        SaveFile = true;
    }
}
