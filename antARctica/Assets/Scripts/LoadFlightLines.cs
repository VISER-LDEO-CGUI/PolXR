using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class LoadFlightLines : MonoBehaviour
{
    public Transform Container;
    public GameObject radarMark;
    public GameObject DEM;
    public GameObject gridLine;

    public void Start()
    {
        LoadFlightLine("20100324_01"); // TODO: replace with menu option
    }

    public void LoadFlightLine(string line_id)
    {
        // Load the data
        UnityEngine.Object[] meshes = Resources.LoadAll("Radar3D/Radar/" + line_id);
        GameObject[] polylines = createPolylineObjects(line_id);

        for (int i = 0; i < meshes.Length; i++)
        {
            // Select mesh
            GameObject meshForward = Instantiate(meshes[i] as GameObject);
            GameObject meshBackward = Instantiate(meshes[i] as GameObject);
            GameObject line = polylines[i];

            // Texture the other side of the mesh
            MeshFilter meshFilter = meshBackward.GetComponent<MeshFilter>();
            Mesh meshUnderlying = meshFilter.mesh;
            int[] triangles = meshUnderlying.triangles;
            for (int j = 0; j < triangles.Length; j += 3)
            {
                int temp = triangles[j];
                triangles[j] = triangles[j + 1];
                triangles[j + 1] = temp;
            }
            meshUnderlying.triangles = triangles;

            // Naming stuff
            meshForward.name = meshForward.name.Replace("(Clone)", "");
            meshBackward.name = "_" + meshForward.name;
            line.name = $"FL_{meshForward.name.Substring(5)}";

            // Create a parent to use for RadarEvents3D
            GameObject parent = new GameObject("GRP_" + meshForward.name);
            parent.transform.SetParent(Container);
            parent.AddComponent<RadarEvents3D>();
            parent.AddComponent<BoxCollider>();
            //parent.AddComponent<BoundsControl>();
            parent.transform.localScale = new Vector3(1, 1, 1);
            parent.transform.localPosition = new Vector3(0, 0, 0);
            parent.transform.rotation = Quaternion.identity;

            // Create a parent to group both radargram object
            GameObject radargram = new GameObject("OBJ_" + meshForward.name);

            // Set the children
            line.transform.parent = parent.transform;
            radargram.transform.parent = parent.transform;
            meshForward.transform.parent = radargram.transform;
            meshBackward.transform.parent = radargram.transform;

            // Place children properly in relation to the DEM
            Quaternion q = line.transform.rotation;
            line.transform.rotation = Quaternion.Euler(-90f, 0, 180f);

            // Create and place the radar mark for the minimap
            Vector3 position = meshForward.transform.position + meshForward.transform.localPosition; // TODO: this
            GameObject mark = Instantiate(radarMark, position, Quaternion.identity, parent.transform);

        }
    }

    public GameObject[] createPolylineObjects(string line_id)
    {
        // Load the file
        string filename = "FlightLine_" + line_id + ".obj";
        string path = Path.Combine(Application.dataPath, "Resources/Radar3D/Polylines", filename).Replace('\\', '/');
        string allText = File.ReadAllText(path);

        // Split up the text by object definition
        GameObject[] polylines = new GameObject[Regex.Matches(allText, "o ").Count];
        int p = 0;
        string[] objects = allText.Split("\no ");

        foreach (string objectText in objects) {

            // Ensure we're looking at an object definition
            if (Regex.Matches(objectText, "\nv ").Count == 0) continue;

            // Instantiate the Game Object
            GameObject line = Instantiate(gridLine);

            // Create arrays to store the vertices as per the .obj specification
            Vector3[] vertices = new Vector3[Regex.Matches(objectText, "v ").Count];
            int v = 0;

            // Read the file one line at a time
            string[] text = objectText.Split('\n');
            for (int i = 0; i < text.Length; i++)
            {
                string textline = text[i];

                // Vertex
                if (textline.StartsWith("v "))
                {
                    // Extract coordinates as floats
                    string[] vertexComponents = textline.Split(' ');
                    float x = float.Parse(vertexComponents[1]);
                    float y = float.Parse(vertexComponents[3]); // because unity and blender have different y/z
                    float z = float.Parse(vertexComponents[2]);

                    // Store the coordinates
                    Vector3 vertex = new Vector3(x, y, z);
                    vertices[v++] = vertex;
                }
            }

            // Apply rendering
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.positionCount = vertices.Length;
            lineRenderer.SetPositions(vertices);

            // Add colors
            lineRenderer.startColor = new Color(1.0f, 0.5f, 0f);
            lineRenderer.endColor = lineRenderer.startColor;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Store the polyline
            polylines[p++] = line;

        }

        Destroy(gridLine);
        
        return polylines;
    }

}