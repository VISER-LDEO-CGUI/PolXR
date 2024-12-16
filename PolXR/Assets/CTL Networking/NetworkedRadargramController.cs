using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.XR.CoreUtils;
using UnityEngine;

public class NetworkedRadargramController : NetworkBehaviour
{

    //[Networked]
    //public bool isVisiable { get; set; }
    //[Networked]
    //public NetworkObject surface { get; set; }
    //[Networked]
    //public NetworkObject bottom { get; set; }

    //[SerializeField] GameObject SurfaceDEM;
    //[SerializeField] GameObject BottomDEM;
    //MeshRenderer surfaceMeshRenderer;
    //MeshRenderer bottomMeshRenderer;
    MeshRenderer radargramMesh;


    [Networked]
    public bool spawnedProjectile { get; set; }
    [Networked]
    public bool bottomSurfaceToggle { get; set; }
    [Networked]
    public bool meshVisible { get; set; }

    private ChangeDetector _changeDetector;
    public string toggleName;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    //// test whether using start or awake
    //private void Awake()
    //{
    //    
    //    // gpt
    //    UpdateVisibility();
    //}

    private void Awake()
    {
        //surfaceMeshRenderer = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>();
        //bottomMeshRenderer = gameObject.transform.GetChild(1).GetComponent<MeshRenderer>();
        GameObject data = gameObject.transform.GetChild(0).gameObject;
        radargramMesh = data.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
    }


    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                //case nameof(spawnedProjectile):
                //    //_material.color = Color.white;
                //    //if (toggleName.Equals("MEASURES_NSIDC-0715-002"))
                //    //{
                //    surfaceMeshRenderer.enabled = !surfaceMeshRenderer.enabled;
                //    //}
                //    //else
                //    //{

                //    //}
                //    //surface.gameObject.SetActive(true);
                //    //bottom.gameObject.GetComponent<MeshRenderer>().enabled = false;
                //    break;
                case nameof(bottomSurfaceToggle):
                    gameObject.SetActive(bottomSurfaceToggle);
                    break;

                case nameof(meshVisible):
                    radargramMesh.enabled = !radargramMesh.enabled;
                    break;
            }
        }
        //_material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    }


    // Called when the IsVisible property changes
    public override void FixedUpdateNetwork()
    {
        //if (GetInput(out NetworkInputData data))
        //{
        //    if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
        //    {
        //        gameObject.SetActive(!gameObject.activeSelf);
        //        Debug.Log("Toggle visible or not visible");
        //    }

        //}
        //spawnedProjectile = !spawnedProjectile;
        // Debug.Log("want to change");

        //if (GetInput(out NetworkInputData data))
        //{
        //    // Debug.Log("data came in success");
        //    // data.direction.Normalize();
        //    gameObject.transform.Translate(2, 0, 0);

        //}
    }

    public void radargramToggle()
    {
        //toggleName = name;
        bottomSurfaceToggle = !bottomSurfaceToggle;
        //if (name == "MEASURES_NSIDC-0715-002")
        //{
        //    spawnedProjectile = !spawnedProjectile;
        //}
        //else
        //{
        //    bottomSurfaceToggle = !bottomSurfaceToggle;
        //}

        //Debug.Log("want to change" + toggleName);
    }

    public void meshToggle()
    {
        meshVisible = !meshVisible;
        gameObject.transform.Translate(2, 0, 0);
    }
}
