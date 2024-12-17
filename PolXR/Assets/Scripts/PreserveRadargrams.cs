using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreserveRadargrams : MonoBehaviour
{

    public static PreserveRadargrams Instance {get; private set;}
    private List<GameObject> selectedRadargrams = new List<GameObject>();
    private GameObject radargramsContainer;

    private void Awake() 
    {
        if(Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        radargramsContainer = new GameObject("PersistentRadargramsContainer");
        radargramsContainer.transform.localScale = new Vector3(0.2f,0.2f,0.2f);
        DontDestroyOnLoad(radargramsContainer);
    }

    public void AddRadargram(GameObject radargram)
    {
        if(!selectedRadargrams.Contains(radargram))
        {
            selectedRadargrams.Add(radargram);
            radargram.transform.SetParent(radargramsContainer.transform);

            Debug.Log($"Added radargram to persistence: {radargram.name}");

        }
    }

    public void RemoveRadargram(GameObject radargram)
    {
        if(selectedRadargrams.Contains(radargram)) 
        {
            selectedRadargrams.Remove(radargram);
            Debug.Log($"Radargram removed: {radargram.name}");
        }
    }

    public List<GameObject> GetRadargrams()
    {
        return selectedRadargrams;
    }
}
