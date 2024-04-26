using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    
    public GameObject shape;    // Shape for user position
    protected GameObject mainCamera;   // User camera
    public Vector3 shapeOffset;
    public float heightOffset;
    public GameObject MinimapCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        // shape.transform.parent = playerCam.transform;
        shape.SetActive(true);
        // shape.transform.position =  new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y+heightOffset,
        // mainCamera.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = mainCamera.gameObject.transform.position;
        shape.transform.position =  new Vector3(camPos[0], 5.0f, camPos[2]);
        MinimapCam.transform.position = new Vector3(camPos[0], 15.0f, camPos[2]);
        
    }
}
