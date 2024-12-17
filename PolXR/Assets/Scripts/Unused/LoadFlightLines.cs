using UnityEngine;
using UnityEngine.XR;
//using Microsoft.MixedReality.Toolkit.Input;
//using Microsoft.MixedReality.Toolkit.UI;
//using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
//using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
//using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
//using GLTFast;
//using Oculus.Platform;

public class LoadFlightLines : MonoBehaviour
{
    public Transform Container;
    //public GameObject radarMark;
    public GameObject DEM;
    public GameObject gridLine;

    public GameObject MarkObj3D;
    //public void Awake()
    //{
    //    BetterStreamingAssets.Initialize();
    //    LoadFlightLine("20100324_01"); // TODO: replace with menu option
    //    Debug.Log("Loaded flight line");
    //}
    // public class YourCustomInstantiator : GLTFast.IInstantiator {
    // // Your code here
    // }
    //public async void LoadFlightLine(string line_id)
    //{
    //    // // Load the data // old workflow loading from resources
    //    //UnityEngine.Object[] meshes = Resources.LoadAll(Path.Combine("Radar3D", "Radar", line_id));
    //    Dictionary<string, GameObject> polylines = createPolylineObjects(line_id);
    //    //GameObject prefab = Instantiate(Resources.Load(Path.Combine("Radar3D", "Radar", "RadarContainer")) as GameObject);

    //    // Load the glTF asset from streaming assets folder
    //    byte[] data = BetterStreamingAssets.ReadAllBytes("radar.glb");

    //    var gltf = new GltfImport();
    //    var success = await gltf.Load(data);
    //    UnityEngine.Object[] meshes;

    //    if (success)
    //    {
    //        // Extract meshes and textures
    //        meshes = ExtractMeshes(gltf);
    //    }
    //    else
    //    {
    //        Debug.LogError("Failed to load glTF asset");
    //        return;
    //    }

    //    for (int i = 0; i < meshes.Length; i++)
    //    {
    //        // Create radargram objects
    //        GameObject[] meshBoth = createRadargramObjects(meshes[i]);
    //        GameObject meshForward = meshBoth[0];
    //        GameObject meshBackward = meshBoth[1];
    //        Bounds meshBounds = meshForward.GetComponent<Renderer>().bounds; // cuz we need bounds in world coords

    //        // Select and name line
    //        string key = meshForward.name.Substring(meshForward.name.IndexOf('_', meshForward.name.Length - 5));
    //        GameObject line;
    //        try
    //        {
    //            line = polylines[key];
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.Log("Polyline not found: '" + key + "'");
    //            Destroy(meshForward);
    //            Destroy(meshBackward);
    //            continue;
    //        }
    //        line.name = $"FL_{meshForward.name.Trim().Substring(5)}";

    //        // Create a parent for all the new objects to associate with RadarEvents3D
    //        string parentName = "#" + key; //"GRP_" + meshForward.name;
    //        GameObject parent = new GameObject(parentName);
    //        parent.transform.SetParent(Container);
    //        RadarEvents3D script = parent.AddComponent<RadarEvents3D>();
    //        parent.transform.localScale = new Vector3(1, 1, 1);
    //        parent.transform.localPosition = new Vector3(0, 0, 0);
    //        parent.transform.rotation = Quaternion.identity;
    //        // BoundsControl parentBoundsControl = parent.AddComponent<BoundsControl>();

    //        //turns off gizmos and bounding boxes
    //        //parentBoundsControl.LinksConfig.ShowWireFrame = false;
    //        //parentBoundsControl.RotationHandlesConfig.ShowHandleForX = false;
    //        //parentBoundsControl.RotationHandlesConfig.ShowHandleForY = false;
    //        //parentBoundsControl.RotationHandlesConfig.ShowHandleForZ = false;
    //        //parentBoundsControl.ScaleHandlesConfig.ShowScaleHandles = false;

    //        // Create a parent to group both radargram objects
    //        GameObject radargram = new GameObject("OBJ_" + meshForward.name);
    //        radargram.transform.localPosition = meshBounds.center;
    //        // MeshCollider radarCollider = radargram.AddComponent<MeshCollider>();
    //        // radarCollider.sharedMesh = meshBackward.GetComponent<MeshFilter>().mesh;

    //        // add mesh colliders to each of the mesh forward and backward
    //        MeshCollider meshForwardCollider = meshForward.AddComponent<MeshCollider>();
    //        meshForwardCollider.sharedMesh = meshForward.GetComponent<MeshFilter>().mesh;

    //        MeshCollider meshBackwardCollider = meshBackward.AddComponent<MeshCollider>();
    //        meshBackwardCollider.sharedMesh = meshBackward.GetComponent<MeshFilter>().mesh;


    //        // Organize the children
    //        line.transform.parent = parent.transform;
    //        radargram.transform.parent = parent.transform;
    //        meshForward.transform.parent = radargram.transform;
    //        meshBackward.transform.parent = radargram.transform;

    //        // Place polyline properly in relation to the DEM
    //        line.transform.rotation = Quaternion.Euler(-90f, 0f, 180f);

    //        // Add the correct Bounds Control so that MRTK knows where the objects are
    //        //BoundsControl boundsControl = radargram.AddComponent<BoundsControl>();
    //        //boundsControl.CalculationMethod = BoundsCalculationMethod.ColliderOverRenderer;

    //        //turns off gizmos and bounding boxes
    //        //boundsControl.LinksConfig.ShowWireFrame = false;
    //        //boundsControl.RotationHandlesConfig.ShowHandleForX = false;
    //        //boundsControl.RotationHandlesConfig.ShowHandleForY = false;
    //        //boundsControl.RotationHandlesConfig.ShowHandleForZ = false;
    //        //boundsControl.ScaleHandlesConfig.ShowScaleHandles = false;

    //        BoxCollider boxCollider = radargram.GetComponent<BoxCollider>();
    //        boxCollider.center = new Vector3(0, 0, 0);//meshBounds.center;
    //        boxCollider.size = meshBounds.size;
    //        // boundsControl.BoundsOverride = boxCollider;

    //        // Constrain the rotation axes
    //        //RotationAxisConstraint rotationConstraint = radargram.AddComponent<RotationAxisConstraint>();
    //        //rotationConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;

    //        // Set the parent's BoxCollider to have the same bounds
    //        BoxCollider parentCollider = parent.GetComponent<BoxCollider>();

    //        // Add the correct Object Manipulator so users can grab the radargrams
    //        //radargram.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
    //        //radargram.AddComponent<NearInteractionGrabbable>();
    //        //Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objectManipulator = radargram.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
    //        //objectManipulator.enabled = true;

    //        // Link the parent to the menu
    //        script.Menu = GameObject.Find("Menu");

    //        // Create and place the radar mark for the minimap
    //        Vector3 position = meshForward.transform.position + meshForward.transform.localPosition; // TODO: this
    //        //GameObject mark = Instantiate(radarMark, position, Quaternion.identity, parent.transform);

    //        //GameObject markObj3D = Instantiate(MarkObj3D, position, Quaternion.identity, radargram.transform);
    //        GameObject markObj3D = Instantiate(MarkObj3D, radargram.transform);
    //        markObj3D.transform.localPosition = Vector3.zero;
    //    }

    //    //Drop everything onto the DEM -- this should correlate with the DEM position
    //    Container.transform.localPosition = new Vector3(-10f, 0f, 10f);
    //    foreach (var obj in meshes)
    //    {
    //        Destroy(obj);
    //    }

    //}

    //UnityEngine.Object[] ExtractMeshes(GltfImport gltfImport)
    //{
    //    // Implement the IInstantiator interface methods to extract meshes
    //    // You can use gltfImport.Meshes and gltfImport.MeshResults to access mesh data

    //    // Loop through mesh results
    //    Mesh[] myMeshes = gltfImport.GetMeshes();
    //    List<UnityEngine.Object> meshes = new List<UnityEngine.Object>();

    //    for (int i = 0; i < myMeshes.Length; i++)
    //    {
    //        // Example: Instantiate a Unity GameObject for each mesh
    //        Mesh mesh = myMeshes[i];
    //        int dotIndex = mesh.name.IndexOf('.');
    //        if (dotIndex != -1)
    //        {
    //            mesh.name = mesh.name.Substring(0, dotIndex);
    //        }
    //        if (mesh.name.StartsWith("Data_20100324"))
    //        {
    //            GameObject go = new GameObject(mesh.name);
    //            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
    //            meshFilter.sharedMesh = mesh;
    //            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
    //            meshRenderer.material = gltfImport.GetMaterial(i);


    //            // Rotate the texture 90 degrees to the left
    //            // this is basically swapping out the original .glb texture and using new png images instead
    //            string imgName = mesh.name + ".png";
    //            string path = Path.Combine("HorizontalRadar", imgName);
    //            byte[] fileData = BetterStreamingAssets.ReadAllBytes(path);
    //            Texture2D radarimg = new Texture2D(meshRenderer.material.mainTexture.width, meshRenderer.material.mainTexture.height, TextureFormat.RGBA32, 1, false);
    //            radarimg.LoadImage(fileData);
    //            meshRenderer.material.mainTexture = rotateTexture(radarimg, false);
    //            radarimg.Apply();

    //            meshRenderer.material.mainTexture.filterMode = FilterMode.Bilinear;

    //            // Rotate texture 90 degrees to the right by adjusting UV coordinates
    //            // this is easy and quick but BAD because then line picking doesn't work since the uvs are wrong
    //            // Vector2[] uvs = mesh.uv;
    //            // for (int j = 0; j < uvs.Length; j++)
    //            // {
    //            //     uvs[j] = new Vector2(uvs[j].y, 1 - uvs[j].x);
    //            // }
    //            // mesh.uv = uvs;

    //            // Set the rendering mode to "Opaque" if the material doesn't contain transparency
    //            // This is necessary for the radar to render correctly, otherwise materials in the back
    //            // will appear in the front probably becuase of some transparency problem
    //            if (!meshRenderer.material.shader.name.Contains("Transparent"))
    //            {
    //                meshRenderer.material.SetFloat("_Mode", 0);
    //                meshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
    //                meshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
    //                meshRenderer.material.SetInt("_ZWrite", 1);
    //                meshRenderer.material.DisableKeyword("_ALPHATEST_ON");
    //                meshRenderer.material.DisableKeyword("_ALPHABLEND_ON");
    //                meshRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    //                meshRenderer.material.renderQueue = -1;
    //            }

    //            meshRenderer.receiveShadows = true;
    //            meshFilter.sharedMesh.RecalculateNormals();
    //            // Add more customization as needed
    //            meshes.Add(go);
    //        }
    //    }

    //    return meshes.ToArray();
    //}
    Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
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
        meshForward.name = meshForward.name.Replace("(Clone)", "").Trim();
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
        // this works for the streaming assets folder!!
        string filename = "FlightLine_" + line_id + ".obj";
        byte[] data = BetterStreamingAssets.ReadAllBytes(filename);
        string allText = System.Text.Encoding.Default.GetString(data);

        //string allText = File.ReadAllText(path);

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
                    key = textline.Trim().Substring(textline.IndexOf('_', textline.Length - 5));
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
