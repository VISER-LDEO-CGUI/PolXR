//using Microsoft.MixedReality.Toolkit.Input;
//using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Measurement : MonoBehaviour//, IMixedRealityPointerHandler
{

    public GameObject Menu;
    public GameObject MarkObj;
    public GameObject MeasureObj;
    public GameObject line;

    private bool measureMode = false;
    private int epsg;

    public void Start() { }

    //public void OnPointerDown(MixedRealityPointerEventData eventData)
    //{
    //    measureMode = Menu.GetComponent<MenuEvents>().measureMode() > 0;
    //    UpdateEPSG();
    //    // SetMarkObj(eventData);
    //}
    //public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    //public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    //public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void UpdateEPSG()
    {
        string scene = SceneManager.GetActiveScene().name;
        switch (scene)
        {
            case "antarctica":
                epsg = 3031;
                break;
            case "greenland":
                epsg = 3413;
                break;
            default:
                epsg = 0;
                break;
        }
    }

    //public void SetMarkObj(MixedRealityPointerEventData eventData)
    //{
    //    if (measureMode)
    //    {
    //        MeasureObj.SetActive(true);
    //        MeasureObj.transform.rotation = this.transform.rotation;
    //        MeasureObj.transform.SetParent(this.transform);
    //        MeasureObj.transform.position = eventData.Pointer.Result.Details.Point;
    //        line.SetActive(true);
    //    }
    //    else
    //    {
    //        if (!measureMode)
    //        {
    //            // Clean up!
    //            line.SetActive(false);
    //            MeasureObj.SetActive(false);
    //        }

    //        // The mark
    //        MarkObj.SetActive(true);
    //        MarkObj.transform.rotation = this.transform.rotation;
    //        MarkObj.transform.SetParent(this.transform);
    //        MarkObj.transform.position = eventData.Pointer.Result.Details.Point;
    //    }
    //}
    
    public Transform GetSceneTransform()
    {
        GameObject g = new GameObject();
        Transform t = g.transform;
        UpdateEPSG();

        switch (epsg)
        {
            case 3413:
                t.rotation = Quaternion.Euler(0f, -21.5f, 0f);
                t.localScale = new Vector3(.0001f, .0001f, .001f);
                break;
            default: // includes antarctica... but should it?
                t.rotation = Quaternion.identity;
                t.localScale = new Vector3(.01f, .01f, .01f);
                break;
        }

        return t;
    }

    /*
    public Vector3 CalculateDistance(GameObject a, GameObject b)
    {

    }

    private Vector3 CorrectDistortion(Vector3 a, Vector3 b, int epsg)
    {

    }
    */
}