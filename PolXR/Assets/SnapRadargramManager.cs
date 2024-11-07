using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnapRadargramManager : MonoBehaviour
{
    public RectTransform snapPosition1;
    public RectTransform snapPosition2;

    private int maxSelection = 2;
    private int currentSelectionIndex = 0;

    private List<GameObject> snappedRadargrams = new List<GameObject>();
    private Dictionary<GameObject, OriginalTransform> originalTransforms = new Dictionary<GameObject, OriginalTransform>();

    public void OnRadargramSelected(GameObject radargram)
    {
        if (currentSelectionIndex >= maxSelection)
        {
            Debug.Log("Max selection reached");
            return;
        }

        if(snappedRadargrams.Contains(radargram))
        {
            return;
        }
        if (currentSelectionIndex == 0)
        {
            MoveRadargram(radargram, snapPosition1);
        }
        else if (currentSelectionIndex == 1)
        {
            MoveRadargram(radargram, snapPosition2);
        }

        // Save original transform data to return it later
        originalTransforms[radargram] = new OriginalTransform(radargram.transform.parent, radargram.transform.position, radargram.transform.rotation, radargram.transform.localScale);

        snappedRadargrams.Add(radargram);
        currentSelectionIndex++;
    }

    private void MoveRadargram(GameObject radargram, RectTransform snapPosition)
    {
        radargram.transform.SetParent(snapPosition, false);

        radargram.transform.localPosition = Vector3.zero;
        radargram.transform.localRotation = Quaternion.identity;

        radargram.transform.localScale = Vector3.one;

        Debug.Log("Radargram moved to snap position: " + snapPosition.name);
    }

    public void DeselectRadargrams()
    {
        currentSelectionIndex = 0;

        foreach (var radargram in snappedRadargrams)
        {
            if (radargram != null && originalTransforms.ContainsKey(radargram))
            {
                OriginalTransform originalTransform = originalTransforms[radargram];
                radargram.transform.SetParent(originalTransform.parent);
                radargram.transform.position = originalTransform.position;
                radargram.transform.rotation = originalTransform.rotation;
                radargram.transform.localScale = originalTransform.localScale;
            }
        }

        snappedRadargrams.Clear();
        originalTransforms.Clear();
        Debug.Log("Radargrams deselected");
    }

    private class OriginalTransform
    {
        public Transform parent;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public OriginalTransform(Transform parent, Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            this.parent = parent;
            this.position = position;
            this.rotation = rotation;
            this.localScale = localScale;
        }
    }
}


