using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudySceneManager : MonoBehaviour
{
    public Transform radargramContainer;
    public float radargramScaleFactor = 0.5f;
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
                radargram.transform.localScale = Vector3.one;
                radargramContainer.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
            radargram.SetActive(true);
        }
    }
}
