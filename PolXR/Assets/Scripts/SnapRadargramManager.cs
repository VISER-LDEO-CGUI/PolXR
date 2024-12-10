using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SnapRadargramManager : MonoBehaviour
{

    public GameObject radargramPrefab;
    public Transform contentParent;
    public ScrollRect scrollRect;
    public GameObject StudyButton;

    private int maxSelection = 6;
    private Dictionary<GameObject, GameObject> radargramSprites = new Dictionary<GameObject, GameObject>();
    
    public void OnRadargramSelected(GameObject radargram)
    {
        StudyButton.SetActive(true);
        if (PreserveRadargrams.Instance.GetRadargrams().Count >= maxSelection)
        {
            Debug.Log("max selection reached");
            return;
        }
        if (PreserveRadargrams.Instance.GetRadargrams().Contains(radargram))
        {
            Debug.Log("Radargram already selected");
            return;
        }
        radargram.transform.SetParent(null);
        //add selection to singleton so that it is saved to next scene
        PreserveRadargrams.Instance.AddRadargram(radargram);
        RadarEvents3D radarEvents = radargram.GetComponent<RadarEvents3D>();
        if (radarEvents != null)
        {
            radarEvents.ToggleRadar(true);
        }
        else
        {
            Debug.LogError($"Radargram {radargram.name} does not have RadarEvents3D.");
        }

        //convert 3d radargram to sprite so that it shows up on Unity UI
        Sprite radargramSprite = ConvertRadargramTextureToSprite(radargram);
        
        if (radargramSprite == null)
        {
            Debug.LogError("Failed to convert radargram to sprite.");
            return;
        }
        
        //create new UI element for selected radargram
        GameObject radargramUI = Instantiate(radargramPrefab, contentParent);
        Image radargramImage = radargramUI.GetComponent<Image>();
        
        if(radargramImage != null) 
        {
            radargramImage.sprite = radargramSprite;
        }

        Button deselectButton = radargramUI.transform.Find("DeselectButton").GetComponent<Button>();
        if(deselectButton != null) 
        {
            deselectButton.onClick.AddListener(() =>
            {
                OnRadargramDeselected(radargramUI, radargram);
            });
            Debug.Log("deselect button listener added");
        }
        else 
        {
            Debug.LogError("deselect button not found");
        }
        radargramSprites[radargram] = radargramUI;
        Debug.Log("radargram added");
    }

    private Texture2D CreateTextureCopy(Texture2D sourceTexture)
    {
        Texture2D textureCopy = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);
        textureCopy.SetPixels(sourceTexture.GetPixels());
        textureCopy.Apply();

        return textureCopy;
    }

    private Sprite ConvertRadargramTextureToSprite(GameObject radargram)
    {
        MeshRenderer renderer = radargram.GetComponentInChildren<MeshRenderer>();

        if (renderer == null)
        {
            Debug.LogError($"Radargram '{radargram.name}' does not have a MeshRenderer component.");
            return null;
        }

        Texture2D originalTexture = renderer.material.mainTexture as Texture2D;

        if (originalTexture == null)
        {
            Debug.LogError($"Radargram '{radargram.name}' does not have a valid Texture2D assigned.");
            return null;
        }

        Texture2D textureCopy = CreateTextureCopy(originalTexture); //unique copy 

        return Sprite.Create(textureCopy, new Rect(0, 0, textureCopy.width, textureCopy.height), new Vector2(0.5f, 0.5f));
    }

    public void OnRadargramDeselected(GameObject radargramUI, GameObject radargram) 
    {

        RadarEvents3D radarEvents = radargram.GetComponent<RadarEvents3D>();
        if (radarEvents != null)
        {
            radarEvents.ToggleRadar(false);
        }

        PreserveRadargrams.Instance.RemoveRadargram(radargram);

        if (radargramSprites.ContainsKey(radargram))
        {
            Destroy(radargramSprites[radargram]);
            radargramSprites.Remove(radargram);
        }

        //GameObject radargramObject = radargram.transform.Find("OBJ_" + radargram.name)?.gameObject;
        
        if(PreserveRadargrams.Instance.GetRadargrams().Count < 1) 
        {
            StudyButton.SetActive(false);
        }
    }

    public void LoadStudyScene()
    {
        Debug.Log("LoadStudyScene called");
        var radargrams = PreserveRadargrams.Instance.GetRadargrams();

        if(radargrams.Count == 0)
        {
            Debug.LogWarning("no radargrams to save");
            return;
        }
        foreach (var radargram in radargrams)
        {
            Debug.Log($"Radargram before transition: {radargram.name}, position: {radargram.transform.position}");
        }
        SceneManager.LoadSceneAsync("StudyScene");
    }
}
