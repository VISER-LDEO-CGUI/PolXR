using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared.Utils
{
    public static class LayerUtils
    {
        public static void ApplyLayer(GameObject gameObject, string layerToApplyName, bool applyLayerToChildren = false)
        {
            if (layerToApplyName != "")
            {
                int layer = LayerMask.NameToLayer(layerToApplyName);
                if (layer == -1)
                {
                    Debug.LogError($"Please add a {layerToApplyName} layer. Required by {gameObject.name}");
                }
                else
                {
                    LayerUtils.ApplyLayer(gameObject, layer, applyLayerToChildren);
                }
            }
        }

        public static void ApplyLayer(GameObject go, int layer, bool applyLayerToChildren)
        {
            if (applyLayerToChildren)
            {
                foreach (Transform child in go.transform)
                {
                    ApplyLayer(child.gameObject, layer, applyLayerToChildren);
                }
            }
            go.layer = layer;
        }

        public static void ApplyLayer<T>(GameObject go, int layer, bool applyLayerToChildren) where T : MonoBehaviour
        {
            if (applyLayerToChildren)
            {
                foreach (T child in go.GetComponentsInChildren<T>())
                {
                    child.gameObject.layer = layer;
                }
            }
            go.layer = layer;
        }

        public static void EditCameraCullingMask(Camera c, List<string> layerNamesToAdd, List<string> layerNamesToRemove)
        {
            List<int> layersToAdd = new List<int>();
            List<int> layersToRemove = new List<int>();

            if (layerNamesToAdd != null)
            {
                foreach (var layerName in layerNamesToAdd) {
                    if (layerName != "")
                    {
                        int layer = LayerMask.NameToLayer(layerName);
                        if (layer == -1)
                        {
                            Debug.LogError($"Please add a {layerName} layer. Required by {c.name}");
                        }
                        else
                        {
                            layersToAdd.Add(layer);
                        }
                    }
                }
            }
            if (layerNamesToRemove != null)
            {
                foreach (var layerName in layerNamesToRemove) {
                    if (layerName != "")
                    {
                        int layer = LayerMask.NameToLayer(layerName);
                        if (layer == -1)
                        {
                            Debug.LogError($"Please add a {layerName} layer. Required by {c.name}");
                        }
                        else
                        {
                            layersToRemove.Add(layer);
                        }
                    }
                }
            }
            EditCameraCullingMask(c, layersToAdd, layersToRemove);
        }

        public static void EditCameraCullingMask(Camera c, List<int> layersToAdd, List<int> layersToRemove)
        {
            if (layersToAdd != null)
            {
                foreach (var layer in layersToAdd) c.cullingMask |= (1 << layer);
            }
            if (layersToRemove != null)
            {
                foreach (var layer in layersToRemove) c.cullingMask &= ~(1 << layer);
            }
        }
    }
}
