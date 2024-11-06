using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Radargram : MonoBehaviour
{
    private bool isSelected = false;
    public GameObject meshForward { get; private set; }
    public GameObject meshBackward { get; private set; }

    public void Initialize(GameObject forwardMesh, GameObject backwardMesh)
    {
        meshForward = forwardMesh;
        meshBackward = backwardMesh;

        meshForward.transform.SetParent(transform);
        meshBackward.transform.SetParent(transform);
    }

    public void ApplyModeBehavior(Mode currentMode) 
    {

    }
    public void Highlight(bool isActive)
    {
        if(isSelected == isActive) return;
        isSelected = isActive;

        ApplyHighlightEffect(meshForward, isSelected);
        ApplyHighlightEffect(meshBackward, isSelected);
    }

    private void ApplyHighlightEffect(GameObject mesh, bool enableOutline)
    {
        if(mesh == null) return;

        var outline = mesh.GetComponent<Outline>();
        if(enableOutline)
        {
            if(outline == null) 
            {
                outline = mesh.AddComponent<Outline>();
                outline.effectColor = new Color(0, 1, 0, 0.5f);
                outline.effectDistance = new Vector2(5,5);
            }
        }
        else 
        {
            if(outline != null)
            {
                Destroy(outline);
            }
        }
    }
}
