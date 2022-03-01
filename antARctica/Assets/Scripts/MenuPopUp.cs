using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPopUp : MonoBehaviour
{
    public GameObject Menu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        Menu.SetActive(true);
        Menu.transform.rotation = this.transform.rotation;
        Menu.transform.SetParent(this.transform);
        Menu.transform.position = eventData.Pointer.Result.Details.Point;
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }
}