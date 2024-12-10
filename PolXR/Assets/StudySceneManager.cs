using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudySceneManager : MonoBehaviour
{
    public Transform radargramContainer;
    void Start()
    {
        List<GameObject> radargrams = PreserveRadargrams.Instance?.GetRadargrams();

        if(radargrams == null || radargrams.Count == 0) 
        {
            Debug.LogWarning("No radargrams found in the study scene");
            return;
        }

        foreach(GameObject radargram in radargrams)
        {
            Debug.Log($"radargram in study scene: {radargram.name}");
            if (radargramContainer != null)
            {
                radargram.transform.SetParent(radargramContainer, true);
            }
            radargram.SetActive(true);
        }
    }
}
