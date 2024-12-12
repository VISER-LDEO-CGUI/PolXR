using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StudySceneManager : MonoBehaviour
{
    public Transform radargramContainer;
    public GameObject backToGreenland;
    void Start()
    {
        //retrieve list of radargrams from singleton
        List<GameObject> radargrams = PreserveRadargrams.Instance?.GetRadargrams();
        if(radargramContainer == null)
        {
            GameObject containerObject = new GameObject("RadargramContainer");
            radargramContainer = containerObject.transform;

        }
        if(radargrams == null || radargrams.Count == 0) 
        {
            Debug.LogWarning("No radargrams found in the study scene");
            return;
        }

        foreach(GameObject radargram in radargrams)
        {
            radargram.transform.SetParent(radargramContainer, true);
            radargram.transform.localScale = Vector3.one;
            radargram.SetActive(true);
        }
    }

    public void BackToGreenland()
    {
        SceneManager.LoadSceneAsync("greenland");
    }
}
