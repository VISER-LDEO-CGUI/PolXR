using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;

public class SnapRadargramManager : MonoBehaviour
{
    public Image snapImage1;  // Reference to the Image component for the first radargram
    public Image snapImage2;  // Reference to the Image component for the second radargram

    private int maxSelection = 2;
    private int currentSelectionIndex = 0;

    public void OnRadargramSelected(GameObject radargram)
    {
        if (currentSelectionIndex >= maxSelection)
        {
            Debug.Log("Max selection reached");
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
            snapImage1.gameObject.SetActive(true);  // Make sure the image is visible
        }
        else if (currentSelectionIndex == 1)
        {
            snapImage2.sprite = radargramSprite;
            snapImage2.gameObject.SetActive(true);  // Make sure the image is visible
        }

        currentSelectionIndex++;
    }

    private Texture2D CreateTextureCopy(Texture2D sourceTexture)
    {
        // Create a new Texture2D with the same dimensions as the source
        Texture2D textureCopy = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);

        // Copy the pixels from the source texture to the new one
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

        // Create a unique copy of the texture
        Texture2D textureCopy = CreateTextureCopy(originalTexture);

        // Create a new sprite from the texture copy
        return Sprite.Create(textureCopy, new Rect(0, 0, textureCopy.width, textureCopy.height), new Vector2(0.5f, 0.5f));
    }

    public void DeselectRadargrams()
    {
        currentSelectionIndex = 0;

        // Reset the snap images
        snapImage1.sprite = null;
        snapImage1.gameObject.SetActive(false);  // Hide the image
        snapImage2.sprite = null;
        snapImage2.gameObject.SetActive(false);  // Hide the image

        Debug.Log("Radargrams deselected");
    }
}
