using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CSVPlotter : MonoBehaviour
{
    // Name of the input file, no extension
    public string inputfile;

    // List for holding data from CSV reader
    private List<Dictionary<string, object>> pointList;

    // Indices for columns to be assigned
    public int columnX = 0;
    public int columnY = 1;
    public int columnZ = 2;

    public float scaleFactor = 0.1F;

    // Full column names
    public string xName;
    public string yName;
    public string zName;

    // The prefab for the data points to be instantiated
    public GameObject PointPrefab;
    public Transform Parent;

    private Vector3 parentPos;
    private Vector3 parentScale;
    private Vector3 relativePos;

    // The particle codes take reference from https://answers.unity.com/questions/1153069/how-to-set-particles.html & https://www.it610.com/article/1288857878597279744.htm
    public ParticleSystem CSVPlotting;
    private ParticleSystem.Particle[] CSVPoints;
    public bool UseParticle = false;
    public float ColorMid = -0.2f;
    public float ColorRange = 10.0f;

    // Use this for initialization
    void Start()
    {
        parentPos = Parent.position;
        parentScale = Parent.localScale;
        relativePos = new Vector3();
        Vector3 distance = this.transform.position - parentPos;
        relativePos.x = Vector3.Dot(distance, Parent.right.normalized);
        relativePos.y = Vector3.Dot(distance, Parent.up.normalized);
        relativePos.z = Vector3.Dot(distance, Parent.forward.normalized);

        // Set pointlist to results of function Reader with argument inputfile
        pointList = CSVReader.Read(inputfile);

        //Log to console
        //Debug.Log(pointList);

        // Declare list of strings, fill with keys (column names)
        List<string> columnList = new List<string>(pointList[1].Keys);

        // Print number of keys (using .count)
        // Print number of objects overall - - - - - - - - - - - - - - - - - - i added this
        //Debug.Log("There are " + columnList.Count + " columns in CSV");
        //Debug.Log("There are " + pointList.Count + " items in CSV");

        //foreach (string key in columnList)
            //Debug.Log("Column name is " + key);

        // Assign column name from columnList to Name variables
        xName = columnList[columnX];
        yName = columnList[columnY];
        zName = columnList[columnZ];
        //Debug.Log("X column name is " + xName);
        //Debug.Log("Y column name is " + yName);
        //Debug.Log("Z column name is " + zName);

        if (UseParticle)
        {
            CSVPoints = new ParticleSystem.Particle[pointList.Count];
            var main = CSVPlotting.main;
            Color mainColor = main.startColor.color;
            main.startLifetime = 86400f;
            main.startSpeed = 0f;
            main.maxParticles = pointList.Count;
            CSVPlotting.Emit(pointList.Count);
            CSVPlotting.GetParticles(CSVPoints);

            int i = -1;

            Parallel.ForEach(pointList, point =>
            {
                // Get value in poinList at ith "row", in "column" Name
                float x = System.Convert.ToSingle(point[xName]) * scaleFactor;
                float y = System.Convert.ToSingle(point[yName]);
                float z = System.Convert.ToSingle(point[zName]) * scaleFactor;


                if (x > -9000 & y > -9000 & z > -9000)
                {
                    // Instantiate the prefab with coordinates defined above
                    // If want to edit color, edit the second line
                    i += 1;
                    CSVPoints[i].position = new Vector3(x, y, z);
                    CSVPoints[i].startColor = new Color(mainColor.r, (y - ColorMid) / ColorRange, mainColor.b, 1.0f);
                }
            }
            );

            CSVPlotting.SetParticles(CSVPoints, i);
            Debug.Log(i.ToString() + " particles set.");
        }
        else
        {
            //Loop through Pointlist
            for (var i = 0; i < pointList.Count; i++)
            {
                // Get value in poinList at ith "row", in "column" Name
                float x = System.Convert.ToSingle(pointList[i][xName]) * scaleFactor;
                float y = System.Convert.ToSingle(pointList[i][yName]);
                float z = System.Convert.ToSingle(pointList[i][zName]) * scaleFactor;

                if (x > -9000 & y > -9000 & z > -9000)
                {
                    //instantiate the prefab with coordinates defined above
                    //GameObject tempSphere = Instantiate(PointPrefab, new Vector3(x * scaleFactor, y * scaleFactor, z * scaleFactor), Quaternion.identity, Parent.transform);
                    //tempSphere.transform.localPosition = new Vector3(x * scaleFactor, y * scaleFactor, z * scaleFactor);
                    GameObject tempSphere = Instantiate(PointPrefab, new Vector3(x, y, z) + Parent.position, Quaternion.identity, Parent);
                    //Debug.Log(string.Format("Coord: {0}, {1}, {2}", tempSphere.transform.localPosition.x, tempSphere.transform.localPosition.y, tempSphere.transform.localPosition.z));
                }
            }
        }
    }

    void Update()
    {
        Vector3 curRotation = Parent.rotation.eulerAngles;
        Vector3 curScale = Parent.localScale;
        Vector3 relativeScale = new Vector3(curScale.x / parentScale.x, curScale.y / parentScale.y, curScale.z / parentScale.z);
        Vector3 curRelativePos = Quaternion.Euler(curRotation.x, curRotation.y, curRotation.z) * new Vector3(relativePos.x * relativeScale.x, relativePos.y * relativeScale.y, relativePos.z * relativeScale.z);

        this.transform.rotation = Parent.rotation;
        this.transform.position = Parent.position + curRelativePos;
        if (UseParticle)
        {
            CSVPlotting.transform.localScale = relativeScale;
        }
        else
        {
            this.transform.localScale = relativeScale;
        }
    }
}