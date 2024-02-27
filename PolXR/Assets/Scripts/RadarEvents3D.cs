using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public class RadarEvents3D : RadarEvents, IMixedRealityPointerHandler
{

    // The scientific objects
    public GameObject radargrams;
    public GameObject flightline;

    public GameObject meshForward;
    public GameObject meshBackward;
    public GameObject gridLine;

    //public GameObject MarkObj3D;

    // this is used to store all the picked lines for every radargram. 
    //  it will be used for exporting the picked lines along with the radargram
    //private List<int[,]> linecoordsxyList = new List<int[,]>();


    // Start is called before the first frame update
    void Start()
    {

        // Grab relevant objects
        flightline = this.transform.GetChild(1).gameObject;
        radargrams = this.transform.GetChild(2).gameObject;
        radarMark = this.transform.GetChild(3).gameObject;

        meshForward = radargrams.transform.GetChild(0).gameObject;
        meshBackward = radargrams.transform.GetChild(1).gameObject;
        MarkObj3D = radargrams.transform.GetChild(3).gameObject;

        // Store initial values
        scaleX = radargrams.transform.localScale.x;
        scaleY = radargrams.transform.localScale.y;
        scaleZ = radargrams.transform.localScale.z;
        position = radargrams.transform.localPosition;
        rotation = radargrams.transform.eulerAngles;

        // Add manipulation listeners to the radargrams
        //radargrams.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>().OnManipulationStarted.AddListener(Select);

        // Set objects to their starting states
        radarMark.SetActive(false);
        TogglePolyline(true, false);
        ToggleRadar(false);

    }

    public void TogglePolyline(bool toggle, bool selectRadar)
    {
        // Actually toggle the polyline
        flightline.SetActive(toggle);
        if (!toggle)
        {
            loaded = false;
            return;
        }

        // Render line based on inputs
        LineRenderer lineRenderer = flightline.GetComponent<LineRenderer>();

        // Set color based on selection
        lineRenderer.startColor = lineRenderer.endColor = loaded ?
            selectRadar ?
                new Color(0.1098f, 0.8196f, 0.0588f)       // loaded and selected - Light Green
                : new Color(0.1098f, 0.2705f, 0.0980f)    // loaded, not selected - Dark Green
            : new Color(0.1647f, 0.1647f, 0.1647f);        // not loaded - Dark Gray

    }

    // Turn on/off the 3D surfaces and associated colliders
    public new void ToggleRadar(bool toggle)
    {
        this.transform.GetComponent<BoxCollider>().enabled = !loaded;
        this.transform.GetComponent<BoundsControl>().enabled = toggle;
        radargrams.SetActive(toggle);
        loaded = toggle;
        MarkObj3D.SetActive(true);
        //MarkObj.gameObject.SetActive((MarkObj.transform.parent == this.transform) && toggle);
    }

    // Checks if the radargram is currently loaded
    public bool isLoaded()
    {
        return flightline.GetComponent<LineRenderer>().startColor != new Color(1f, 1f, 0f);
    }

    public void Select()
    {
        // Update the menu
        SychronizeMenu();

        // Select the flightline portion
        foreach (RadarEvents3D sibling in this.transform.parent.gameObject.GetComponentsInChildren<RadarEvents3D>())
        {
            sibling.TogglePolyline(true, false);
        }
        TogglePolyline(true, true);
    }
    private void Select(ManipulationEventData eventData)
    {
        Select();
    }

    // Show the menu and mark and update the variables
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // Only load the images when selected
        ToggleRadar(true);

        // Show that the object has been selected
        Select();

        if (Menu.transform.GetComponent<MenuEvents>().isLinePickingMode){
            doLinePicking(eventData);
        }

        Debug.Log("on pointer down is firing");

    }
    private void doLinePicking(MixedRealityPointerEventData eventData)
    {
        //maybe try commenting out this line
        Ray ray = new Ray(eventData.Pointer.Result.Details.Point, -eventData.Pointer.Result.Details.Normal);
        RaycastHit hit;
        RaycastHit[] hits;

        hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
        {
            //sort hits list by distance closest to furthest
            Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));

            foreach (RaycastHit obj in hits)
            {
                if (obj.transform.name.StartsWith("Data") || obj.transform.name.StartsWith("_Data"))
                { //if it hits mesh forward first

                    DrawMarkObj(obj);
                    Vector2 uvCoordinates = obj.textureCoord;

                    Vector3[] worldcoords = GetLinePickingPoints(uvCoordinates, meshForward, obj.transform.name);
                    //draw line
                    DrawPickedPointsAsLine(worldcoords);
                    break;
                }
            }
        }
    }

    // Unused functions
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData){  }
    private void LateUpdate() { }

    // Gets the scale of the radargram
    public new Vector3 GetScale()
    {
        return new Vector3(radargrams.transform.localScale.x, radargrams.transform.localScale.y, radargrams.transform.localScale.z);
    }

    // Change the transparency of the radar images. "onlyLower" used for setting radar only to more transparent level
    public new void SetAlpha(float newAlpha, bool onlyLower = false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        for (int i = 0; i < 2; i++)
        {
            if(newAlpha == 1)
                ToOpaqueMode(radargrams.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material);
            else
                ToFadeMode(radargrams.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material);
            Color color = radargrams.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().materials[0].color;
            color.a = newAlpha;
            radargrams.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().materials[0].color = color;
        }
    }

    private static void ToOpaqueMode(Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    private static void ToFadeMode(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    // Just resets the radar transform
    public void ResetTransform()
    {
        radargrams.transform.localPosition = position;
        radargrams.transform.localRotation = Quaternion.Euler(rotation);
        radargrams.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    // Reset the radar as if it had not been loaded
    public void ResetRadar()
    {
        // Return the radargrams to their original position
        ResetTransform();

        // Turn the radargrams off
        ToggleRadar(false);

        // Ensure the flightline is still on
        TogglePolyline(true, false);

        // Turn off the radar mark
        radarMark.SetActive(false);
    }

    public void DrawMarkObj(RaycastHit obj)
    {
        Vector3 localPosition = radargrams.transform.InverseTransformPoint(obj.point);

        MarkObj3D.SetActive(true);
        MarkObj3D.transform.rotation = radargrams.transform.rotation;
        MarkObj3D.transform.localPosition = localPosition;

        //draw a sphere at the point of intersection
        //sphere uses world coordinates
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 0.01f;
        sphere.transform.position = obj.point;

    }

    public Vector3[] GetLinePickingPoints(Vector2 uv, GameObject curmesh, string imgname)
    {
        bool isForward = true;
        if (imgname.StartsWith("_"))
        { //is mesh backward
            imgname = imgname.Substring(1);
            isForward = false;
        }
        imgname = imgname + ".png";

        //read in image
        //these ones are horizontal
        // the reason why we don't just directly use the images for the radargram mesh textures is because they are rotated
        string path = Path.Combine(Application.dataPath, "Resources/Radar3D/HorizontalRadar", imgname).Replace('\\', '/');
        
        // Note to future self: do cost benefit analysis of using texture maps vs bitmaps to read in images
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2); 
        texture.LoadImage(fileData);
        
        if (texture == null){
            Debug.Log("Couldn't load in radar image");
        }

        int h = (int)texture.height;
        int w = (int)texture.width;

        // Line picking
        
        int windowSize = 21;
        int halfWin = (int)windowSize / 2;

        //this is correct when origin is top left
        int beginX = (int)(w * uv.y);
        int beginY = (int)(h * uv.x);

        //unity texture has origin at bottom left >:(
        int prevX = beginX;
        int prevY = h - beginY;

        //for debugging reasons we have 2 arrays, but linecoordsxy doesn't really get used
        //it stores the x,y, coordinates of the picked points
        //linecoordsxy will be important for exporting the coordinates
        //we use the uvs array one to draw the line in 3d in Unity

        int[,] linecoordsxy = new int[w, 2];
        Vector2[] uvs = new Vector2[w];

        int maxlocalval = 0;
        int maxlocaly = 0;

        int j = beginX;

        //populate linecoordsxy and uvs with x,y coordinates that are on the line and their corresponding uvs
        //note that coordinate system has shifted with origin at bottom left

        //run loop for all pixels to the right of the picked point
        for (int col = beginX; col < w; col++)
        {
            for (int i = prevY - halfWin; i <= prevY + halfWin; i++)
            {
                byte g = (byte)(255 * texture.GetPixel(col, i).g); //since the image is in black and white, any channel will return the same illuminosity value
                if (maxlocalval < g)
                {
                    maxlocalval = g;
                    maxlocaly = i;
                }
            }
            linecoordsxy[j, 0] = col; // setting x
            linecoordsxy[j, 1] = h - maxlocaly; //setting y, transformed back to origin in top left
            prevY = maxlocaly;
            uvs[j] = new Vector2((float)linecoordsxy[j, 1]/h, (float)linecoordsxy[j, 0]/w); 
            maxlocalval = 0;
            j++;
        }
        
        //run loop again for pixels on the left of the picked point
        maxlocalval = 0;
        maxlocaly = 0;
        prevY = h - beginY;
        j = beginX -1;
        for (int col = beginX -1; col >= 0; col--)
        {
            for (int i = prevY - halfWin; i <= prevY + halfWin; i++)
            {
                byte g = (byte)(255 * texture.GetPixel(col, i).g); //since the image is in black and white, any channel will return the same illuminosity value
                if (maxlocalval < g)
                {
                    maxlocalval = g;
                    maxlocaly = i;
                }
            }
            linecoordsxy[j, 0] = col; // setting x
            linecoordsxy[j, 1] = h - maxlocaly; //setting y, transformed back to origin in top left
            prevY = maxlocaly;
            uvs[j] = new Vector2((float)linecoordsxy[j, 1]/h, (float)linecoordsxy[j, 0]/w); 
            maxlocalval = 0;
            j--;
        }


        Vector3[] worldcoords = new Vector3[w];
        //convert list of uv coordinates to world coords
        for (int i = 0; i <w; i++)
        {
            worldcoords[i] = UvTo3D(uvs[i], curmesh.GetComponent<MeshFilter>().mesh, curmesh.transform);
        }
        //linecoordsxyList.Add(linecoordsxy);
        return worldcoords;
    }

    public Vector3 UvTo3D(Vector2 uv, Mesh mesh, Transform transform)
    {
        int[] tris = mesh.triangles;
        Vector2[] uvs = mesh.uv;
        Vector3[] verts = mesh.vertices;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector2 u1 = uvs[tris[i]];      
            Vector2 u2 = uvs[tris[i + 1]];
            Vector2 u3 = uvs[tris[i + 2]];

            // Calculate triangle area - if zero, skip it
            float a = Area(u1, u2, u3);
            if (a == 0)
                continue;

            // Calculate barycentric coordinates of u1, u2, and u3
            // If any is negative, point is outside the triangle: skip it
            float a1 = Area(u2, u3, uv) / a;
            if (a1 < 0)
                continue;

            float a2 = Area(u3, u1, uv) / a;
            if (a2 < 0)
                continue;

            float a3 = Area(u1, u2, uv) / a;
            if (a3 < 0)
                continue;

            // Point inside the triangle - find mesh position by interpolation
            Vector3 p3D = a1 * verts[tris[i]] + a2 * verts[tris[i + 1]] + a3 * verts[tris[i + 2]];

            // Return it in world coordinates
            return transform.TransformPoint(p3D);
        }

        // Point outside any UV triangle
        return Vector3.zero;
    }

    private float Area(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 v1 = p1 - p3;
        Vector2 v2 = p2 - p3;
        return (v1.x * v2.y - v1.y * v2.x) / 2;
    }


    public void DrawPickedPointsAsLine(Vector3[] worldcoords)
    {
        List<Vector3> filteredCoords = worldcoords.Where(coord => coord != Vector3.zero).ToList();
        GameObject lineObject = new GameObject("Polyline");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        // Set LineRenderer properties
        lineRenderer.positionCount = filteredCoords.Count;
        lineRenderer.startWidth = 0.02f; // Adjust the width as needed
        lineRenderer.endWidth = 0.02f;   // Adjust the width as needed

        // Set positions for the line
        lineRenderer.SetPositions(filteredCoords.ToArray());

        // Set the color of the line using the Unlit/Color shader
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        Color lineColor = new Color(0.2f, 0.2f, 1f);
        lineRenderer.SetColors(lineColor,lineColor);

        // after drawing the line in world space, we now give it a parent (the corresponding radargram)
        // we also turn useWorldSpace to false so that it will move alongside the radargram
        lineRenderer.transform.SetParent(radargrams.transform, true);
        lineRenderer.useWorldSpace = false;



        // //trail renderer for renderering a tube
        // Debug.Log("Before filtering: " + string.Join(", ", worldcoords.Select(coord => coord.ToString()).ToArray()));

        // List<Vector3> filteredCoords = worldcoords.Where(coord => coord != Vector3.zero).ToList();
        // // Debug.Log("After filtering: " + string.Join(", ", filteredCoords.Select(coord => coord.ToString()).ToArray()));
        
        // GameObject tubeObject = new GameObject("Tube");
        // TrailRenderer trailRenderer = tubeObject.AddComponent<TrailRenderer>();

        // float tubeWidth = 0.02f;
        // //Color tubeColor = Color.blue; 

        // // Set TrailRenderer properties
        // trailRenderer.time = 1000f; // Set a long time to make the trail persistent
        // trailRenderer.startWidth = tubeWidth; // Set the width of the tube
        // trailRenderer.endWidth = tubeWidth;
        // trailRenderer.material = new Material(Shader.Find("Standard")); // You can replace this with your own shader or material
        // trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // Color tubeColor = new Color(0.2f, 0.2f, 1f);
        
        // trailRenderer.startColor = tubeColor;
        // trailRenderer.endColor = tubeColor;

        // // Set positions for the trail
        // tubeObject.transform.position = filteredCoords[0];
        // Vector3[] positions = filteredCoords.ToArray();
        // Debug.Log("After conversion: " + string.Join(", ", positions.Select(coord => coord.ToString()).ToArray()));
        // trailRenderer.AddPositions(positions);




        // This draws spheres at each picked point instead of a connected polyline
        // foreach(Vector3 worldcoord in worldcoords){
        //     var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     sphere.transform.localScale = Vector3.one * 0.01f;
        //     sphere.transform.position = worldcoord;
        // }
        
    }

    public void saveRadargram(){

    }
    

}
