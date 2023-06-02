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
            GameObject mesh = Instantiate(meshes[i] as GameObject);
            GameObject line = polylines[i];

            // Naming stuff
            mesh.name = mesh.name.Replace("(Clone)", "");
            line.name = $"FL_{mesh.name.Substring(5)}";

            // Create a parent to use for RadarEvents3D
            GameObject parent = new GameObject("GRP_" + mesh.name);
            parent.transform.SetParent(Container);
            parent.AddComponent<RadarEvents3D>();
            parent.AddComponent<BoxCollider>();
            //parent.AddComponent<BoundsControl>();
            parent.transform.localScale = new Vector3(1, 1, 1);
            parent.transform.localPosition = new Vector3(0, 0, 0);
            parent.transform.rotation = Quaternion.identity;

            // Set the children
            mesh.transform.parent = parent.transform;
            line.transform.parent = parent.transform;

            // Place children properly in relation to the DEM
            mesh = positionOnDEM(mesh, DEM);
            line = positionOnDEM(line, DEM);

            // Create and place the radar mark for the minimap
            Vector3 position = mesh.transform.position + mesh.transform.localPosition; // TODO: this
            GameObject mark = Instantiate(radarMark, position, Quaternion.identity, parent.transform);

        }
    }

    public GameObject positionOnDEM(GameObject obj, GameObject DEM)
    {
        obj.transform.localPosition = DEM.transform.localPosition;
        obj.transform.rotation = DEM.transform.rotation;
        obj.transform.localScale = DEM.transform.localScale;
        return obj;
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
                    float y = float.Parse(vertexComponents[2]);
                    float z = float.Parse(vertexComponents[3]);

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
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Store the polyline
            polylines[p++] = line;

        }

        return polylines;
    }

}