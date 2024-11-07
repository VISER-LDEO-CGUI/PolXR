using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;

public class SnapRadargramManager : MonoBehaviour
{
    public Image snapImage1;  
    public Image snapImage2;  

    private int maxSelection = 2;
    private int currentSelectionIndex = 0;
    private Dictionary<int, GameObject> selectedRadargrams = new Dictionary<int, GameObject>();

    public void OnRadargramSelected(GameObject radargram)
    {
        if (currentSelectionIndex >= maxSelection)
        {
            Debug.Log("Max selection reached");
            return;
        }

        if(selectedRadargrams.ContainsValue(radargram))
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

        if (currentSelectionIndex == 0)
        {
            snapImage1.sprite = radargramSprite;
            snapImage1.gameObject.SetActive(true); 
            selectedRadargrams[currentSelectionIndex] = radargram;
        }
        else if (currentSelectionIndex == 1)
        {
            snapImage2.sprite = radargramSprite;
            snapImage2.gameObject.SetActive(true); 
            selectedRadargrams[currentSelectionIndex] = radargram; 
        }

        currentSelectionIndex++;
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

    public void DeselectRadargrams()
    {
        currentSelectionIndex = 0;

        snapImage1.sprite = null;
        snapImage1.gameObject.SetActive(false);  
        snapImage2.sprite = null;
        snapImage2.gameObject.SetActive(false);  

        Debug.Log("Radargrams deselected");
    }
}
