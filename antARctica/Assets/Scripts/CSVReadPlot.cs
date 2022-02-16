using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CSVReadPlot : MonoBehaviour
{

    public float scaleFactor = 1;

    // The prefab for the data points to be instantiated
    public GameObject PointPrefab;
    public Transform Parent;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("running");
        string[] files = System.IO.Directory.GetFiles("Assets/Resources/SplitByLine_All_Lines_DICE_SurfElev/");
        Debug.Log(files.Length);
        //Parallel.ForEach(files, file =>
        for(int j = 0; j < files.Length; j++)
        {
            //data = System.IO.File.ReadAllText("Assets/Resources/All_Lines_DICE_SurfElev.csv").Split("\n"[0]);
            //data = System.IO.File.ReadAllText("Assets/Resources/SplitByLine_All_Lines_DICE_SurfElev/L870.csv").Split("\n"[0]);

            if (files[j][files[j].Length-1] == 'v')
            {
                Debug.Log(files[j]);

                string[] data = System.IO.File.ReadAllText(files[j]).Split("\n"[0]);

                //GameObject newLine = new GameObject(file);
                //newLine.transform.parent = Parent;

                Debug.Log(data.Length);
                for (int i = 1; i < data.Length; i++)
                {
                    Debug.Log(i);
                    string[] coords = data[i].Split(","[0]);

                    float x = float.Parse(coords[2]) * scaleFactor;
                    float y = float.Parse(coords[5]);
                    float z = float.Parse(coords[3]) * scaleFactor;

                    if (x > -9000 & y > -9000 & z > -9000)
                    {
                        GameObject tempSphere = Instantiate(PointPrefab, new Vector3(x, y, z) + Parent.position, Quaternion.identity);
                    }
                }
                Debug.Log("end");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // try add component if not too slow
    // x is 2
    // y is 5
    // z is 3

}
