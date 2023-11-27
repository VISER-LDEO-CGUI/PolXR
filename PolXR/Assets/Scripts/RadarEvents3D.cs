using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RadarEvents3D : RadarEvents, IMixedRealityPointerHandler
{

    // The scientific objects
    public GameObject radargrams;
    public GameObject flightline;

    public GameObject meshForward;
    public GameObject meshBackward;

    //public GameObject MarkObj3D;

    // Start is called before the first frame update
    void Start()
    {

        // Grab relevant objects
        flightline = this.transform.GetChild(1).gameObject;
        radargrams = this.transform.GetChild(2).gameObject;
        radarMark = this.transform.GetChild(3).gameObject;

        meshForward = radargrams.transform.GetChild(1).gameObject;
        meshBackward = radargrams.transform.GetChild(2).gameObject;
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

    // void Update()
    // {
    //     foreach(var source in CoreServices.InputSystem.DetectedInputSources)
    //     {
    //         // Ignore anything that is not a hand because we want articulated hands
    //         if (source.SourceType == Microsoft.MixedReality.Toolkit.Input.InputSourceType.Hand)
    //         {
    //             foreach (var p in source.Pointers)
    //             {
    //                 if (p is IMixedRealityNearPointer)
    //                 {
    //                     // Ignore near pointers, we only want the rays
    //                     continue;
    //                 }
    //                 if (p.Result != null)
    //                 {
    //                     var startPoint = p.Position;
    //                     var endPoint = p.Result.Details.Point;
    //                     var hitObject = p.Result.Details.Object;
    //                     if (hitObject)
    //                     {
    //                         // var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //                         // sphere.transform.localScale = Vector3.one * 0.01f;
    //                         // sphere.transform.position = endPoint;
    //                     }
    //                 }

    //             }
    //         }
    //     }
    // }

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
                new Color(1f, 0f, 0f)       // loaded and selected
                : new Color(1f, .4f, 0f)    // loaded, not selected
            : new Color(1f, 1f, 0f);        // not loaded

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
        //Debug.Log(eventData.Pointer.Result.Details.Point);
    }

    // Show the menu and mark and update the variables
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        // Only load the images when selected
        ToggleRadar(true);

        // Show that the object has been selected
        Select();

        // Measurement
        //Debug.Log(eventData.Pointer.Result.Details.Point);

        Debug.Log("on pointer down is firing");
        // new ray shooting
        Ray ray = new Ray(eventData.Pointer.Result.Details.Point, -eventData.Pointer.Result.Details.Normal);
        RaycastHit hit;
        RaycastHit[] hits;
       
        Debug.DrawRay(ray.origin, ray.direction*2000);
        hits = Physics.RaycastAll(ray);
        Debug.Log("number of items hit: "+hits.Length);
         if (hits.Length >0)
        {
            Debug.Log("ray fired and hit something, names: ");
        
            //sort hits list by distance closest to furthest
            Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
            // Check if the hit object is the meshBackward

            foreach(RaycastHit obj in hits)
            {
                Debug.Log(obj.collider.gameObject.name);
                if (obj.transform.name.StartsWith("Data")){ //if it hits mesh forward first
                    Debug.Log("hit mesh forward");
                    Vector3 localPosition = radargrams.transform.InverseTransformPoint(obj.point);
                    Debug.Log("Local Coordinates of meshBackward: " + localPosition);
                    Debug.Log("original position"+MarkObj3D.transform.position);

                    MarkObj3D.SetActive(true);
                    MarkObj3D.transform.rotation = radargrams.transform.rotation;
                    //MarkObj3D.transform.SetParent(radargrams.transform, true);
                    MarkObj3D.transform.localPosition = localPosition;
                    Debug.Log("curr markobj position: "+MarkObj3D.transform.position);

                    //draw a sphere at the point of intersection
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.localScale = Vector3.one * 0.01f;
                    sphere.transform.position = obj.point;
                    Debug.Log("sphere dot position: "+sphere.transform.position);

                    //radargrams.GetComponent<MeshCollider>().GetComponent<MeshFilter>().mesh;
                    // Mesh mesh = meshForward.GetComponent<MeshCollider>().sharedMesh;
                    // List <Vector2> uv = new List <Vector2>();
                    // mesh.GetUVs(0,uv);
                    // Debug.Log(uv); 
                    Vector2 uvCoordinates = obj.textureCoord;
                    Debug.Log("UV Coordinates: " + uvCoordinates);

                    break;
                // } else if(obj.transform == meshBackward.transform){
                } else if(obj.transform.name.StartsWith("_Data")){ //if ray hits mesh backward
                    Vector3 localPosition = radargrams.transform.InverseTransformPoint(obj.point);
                    Debug.Log("Local Coordinates of meshBackward: " + localPosition);
                    Debug.Log("original position"+MarkObj3D.transform.position);

                    //none of this is working and i don't know why
                    MarkObj3D.SetActive(true);
                    MarkObj3D.transform.rotation = radargrams.transform.rotation;
                    //MarkObj3D.transform.SetParent(radargrams.transform, true);
                    MarkObj3D.transform.localPosition = localPosition;
                    Debug.Log("curr markobj position: "+MarkObj3D.transform.position);

                    //draw a sphere at the point of intersection
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.localScale = Vector3.one * 0.01f;
                    sphere.transform.position = obj.point;
                    Debug.Log("sphere dot position: "+sphere.transform.position);

                    //radargrams.GetComponent<MeshCollider>().GetComponent<MeshFilter>().mesh;
                    Mesh mesh = meshForward.GetComponent<MeshCollider>().sharedMesh;
                    List <Vector2> uv = new List <Vector2>();
                    mesh.GetUVs(0,uv);
                    Debug.Log(uv); 
                    
                    break;
                }
            }
                // Get the local coordinates of the hit point on the mesh
        }
        // Debug.Log("Current position of MarkObj3D: " + MarkObj3D.transform.position);

        // MarkObj3D.transform.rotation = radargrams.transform.rotation;
        // MarkObj3D.transform.SetParent(radargrams.transform);
        // MarkObj3D.transform.position = eventData.Pointer.Result.Details.Point;
        // Debug.Log(eventData.Pointer.Result.Details.Point);

    }
    // void OnMouseDown()
    // {
    //     Debug.Log("on mouse down is firing");
    //     // new ray shooting
    //     Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
    //     RaycastHit hit;
    //     RaycastHit[] hits;
       
    //     Debug.Log(radargrams);
    //     Debug.DrawRay(ray.origin, ray.direction*20);
    //     hits = Physics.RaycastAll(ray);

    //      if (hits.Length >0)
    //     {
    //         Debug.Log("ray fired and hit something");
        
    //         Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
    //         // Check if the hit object is the meshBackward

    //         foreach(RaycastHit obj in hits)
    //         {
    //             Debug.Log(obj.collider.gameObject.name);
    //             if (obj.transform == radargrams.transform){
    //                 Vector3 localPosition = radargrams.transform.InverseTransformPoint(obj.point);
    //                 Debug.Log("Local Coordinates of meshBackward: " + localPosition);
    //             }
    //         }
    //             // Get the local coordinates of the hit point on the mesh
    //     }
        
    // }

    // Unused functions
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData) {

     }
    private void LateUpdate() { }

    // Gets the scale of the radargram
    public new Vector3 GetScale()
    {
        return new Vector3(radargrams.transform.localScale.x, radargrams.transform.localScale.y, radargrams.transform.localScale.z);
    }

    // Change the transparency of the radar images. "onlyLower" used for setting radar only to more transparent level
    public new void SetAlpha(float newAlpha, bool onlyLower=false)
    {
        if ((onlyLower && alpha > newAlpha) || !onlyLower) alpha = newAlpha;
        for (int i = 0; i < 2; i++)
        {
            Color color = radargrams.transform.GetChild(i).gameObject.GetComponent<Renderer>().material.color;
            color.a = newAlpha;
        }
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

}
