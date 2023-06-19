﻿using UnityEngine;
using UnityEngine.XR;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

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
        Dictionary<string, GameObject> polylines = createPolylineObjects(line_id);
        GameObject prefab = Instantiate(Resources.Load("Radar3D/Radar/RadarContainer") as GameObject);

        for (int i = 0; i < meshes.Length; i++)
        {
            // Create radargram objects
            GameObject[] meshBoth = createRadargramObjects(meshes[i]);
            GameObject meshForward = meshBoth[0];
            GameObject meshBackward = meshBoth[1];

            // Select and name line
            GameObject line = polylines[meshForward.name.Substring(meshForward.name.Length - 4)];
            line.name = $"FL_{meshForward.name.Substring(5)}";

            // Create a parent for all the new objects to associate with RadarEvents3D
            GameObject parent = new GameObject("GRP_" + meshForward.name);
            parent.transform.SetParent(Container);
            parent.AddComponent<RadarEvents3D>();
            parent.transform.localScale = new Vector3(1, 1, 1);
            parent.transform.localPosition = new Vector3(0, 0, 0);
            parent.transform.rotation = Quaternion.identity;
            parent.AddComponent<BoundsControl>();

            // Create a parent to group both radargram objects
            GameObject radargram = new GameObject("OBJ_" + meshForward.name);
            MeshCollider radarCollider = radargram.AddComponent<MeshCollider>();
            radarCollider.sharedMesh = meshBackward.GetComponent<MeshFilter>().mesh;

            // Deal with object manipulation
            BoundsControl boundsControl = radargram.AddComponent<BoundsControl>();
            boundsControl.CalculationMethod = BoundsCalculationMethod.ColliderOverRenderer;
            BoxCollider boxCollider = radargram.GetComponent<BoxCollider>();
            Bounds meshBounds = meshForward.GetComponent<Renderer>().bounds; // cuz we need bounds in world coords
            boxCollider.center = meshBounds.center;
            boxCollider.size = meshBounds.size;
            boundsControl.BoundsOverride = boxCollider;

            radargram.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();

            // Organize the children
            line.transform.parent = parent.transform;
            radargram.transform.parent = parent.transform;
            meshForward.transform.parent = radargram.transform;
            meshBackward.transform.parent = radargram.transform;

            // Place polyline properly in relation to the DEM
            line.transform.rotation = Quaternion.Euler(-90f, 0, 180f);

            // Create and place the radar mark for the minimap
            Vector3 position = meshForward.transform.position + meshForward.transform.localPosition; // TODO: this
            GameObject mark = Instantiate(radarMark, position, Quaternion.identity, parent.transform);
        }
    }

    public GameObject[] createRadargramObjects(UnityEngine.Object obj)
    {
        // Select mesh
        GameObject meshForward = Instantiate(obj as GameObject);
        GameObject meshBackward = Instantiate(obj as GameObject);

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

        // Name meshes
        meshForward.name = meshForward.name.Replace("(Clone)", "");
        meshBackward.name = "_" + meshForward.name;

        // Return meshes
        GameObject[] meshes = new GameObject[2];
        meshes[0] = meshForward;
        meshes[1] = meshBackward;
        return meshes;
    }

    public Dictionary<string, GameObject> createPolylineObjects(string line_id)
    {
        // Load the polyline file
        string filename = "FlightLine_" + line_id + ".obj";
        string path = Path.Combine(Application.dataPath, "Resources/Radar3D/FlightLines", filename).Replace('\\', '/');
        string allText = File.ReadAllText(path);

        // Split up the text by object definition
        Dictionary<string, GameObject> polylines = new Dictionary<string, GameObject>();
        string[] objects = allText.Split("\no ");
        string key = null;

        foreach (string objectText in objects) 
        {

            // Ensure we're looking at an object definition
            if (Regex.Matches(objectText, "\nv ").Count == 0) continue;

            // Instantiate the Game Object
            GameObject line = Instantiate(gridLine);

            // Create an array to store the vertices as per the .obj specification
            Vector3[] vertices = new Vector3[Regex.Matches(objectText, "v ").Count];
            int v = 0;

            // Read the file one line at a time
            string[] text = objectText.Split('\n');
            for (int i = 0; i < text.Length; i++)
            {
                string textline = text[i];

                // Set the key
                if (key is null)
                {
                    key = textline.Substring(textline.Length - 4);
                    continue;
                }

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

            // Add material
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Create collider
            for (int i = 1; i < vertices.Length; i++)
            {
                // Calculate the line segment
                Vector3 a = vertices[i - 1];
                Vector3 b = vertices[i];

                // Add the collider
                BoxCollider collider = line.AddComponent<BoxCollider>();
                collider.isTrigger = true;

                // Set the collider bounds
                collider.center = (a + b) / 2f;
                collider.size = new Vector3(
                    Math.Max(Mathf.Abs(a.x - b.x), 0.2f),
                    Math.Max(Mathf.Abs(a.y - b.y), 0.2f),
                    Math.Max(Mathf.Abs(a.z - b.z), 0.2f)
                );
            }

            // Store the polyline
            polylines.Add(key, line);

            // Reset the key
            key = null;

        }

        // Finish up
        Destroy(gridLine);
        return polylines;
    }

}