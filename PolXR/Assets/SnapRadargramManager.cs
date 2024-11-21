using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;

public class SnapRadargramManager : MonoBehaviour
{

    public GameObject radargramPrefab;
    public Transform contentParent;
    public ScrollRect scrollRect;

    private int maxSelection = 8;
    private List<GameObject> selectedRadargrams = new List<GameObject>();

    public void OnRadargramSelected(GameObject radargram)
    {
        if (selectedRadargrams.Count >= maxSelection)
        {
            Debug.Log("Max selection reached");
            RadarEvents3D radarEvents = radargram.GetComponent<RadarEvents3D>();
            if(radarEvents != null)
            {
                radarEvents.ToggleRadar(false);
            }
            return;
        }

        if(selectedRadargrams.Contains(radargram))
        {
            Debug.Log("radargram already selected");
            return;
        }

        Sprite radargramSprite = ConvertRadargramTextureToSprite(radargram);
        
        if (radargramSprite == null)
        {
            Debug.LogError("Failed to convert radargram to sprite.");
            return;
        }
        
        selectedRadargrams.Add(radargram);
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
        if(selectedRadargrams.Contains(radargram)) 
        {
            selectedRadargrams.Remove(radargram);
            Debug.Log($"radargram deselected in the UI. remaining selections: {selectedRadargrams.Count}");
        }
        Destroy(radargramUI);

        RadarEvents3D radarEvents = radargram.GetComponent<RadarEvents3D>();
        if(radarEvents != null) 
        {
            radarEvents.ToggleRadar(false);
            radarEvents.TogglePolyline(true,false);
            Debug.Log($"radargram {radargram.name} deselected in the SCENE.");
        }
    }
}
