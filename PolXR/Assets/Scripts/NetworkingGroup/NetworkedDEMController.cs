using Fusion;
using UnityEngine;

public class NetworkedDEMController : NetworkBehaviour
{
    [Networked]
    public bool isVisiable { get; set; }
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public MeshRenderer demMeshRenderer;
    //// test whether using start or awake
    //private void Awake()
    //{
    //    demMeshRenderer = GetComponent<MeshRenderer>();
    //    // gpt
    //    UpdateVisibility();
    //}

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
        gameObject.SetActive(!gameObject.activeSelf);
        Debug.Log("want to change");
    }

    // Updates the visibility of the DEM
    private void UpdateVisibility()
    {
        if (demMeshRenderer != null)
        {
            demMeshRenderer.enabled = isVisiable;
        }
        gameObject.SetActive(isVisiable);
    }

    // Method to toggle visibility (only the host should call this)
    public void ToggleVisibility(bool visibility)
    {
        if (HasStateAuthority)
        {
            isVisiable = visibility;
        }
    }
}