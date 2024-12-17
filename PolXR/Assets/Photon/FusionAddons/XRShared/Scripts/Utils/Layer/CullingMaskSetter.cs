using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fusion.XR.Shared.Utils
{
    public class CullingMaskSetter : MonoBehaviour
    {
        [SerializeField] 
        List<string> layerNamesToAdd = new List<string>();
        [SerializeField]
        List<string> layerNamesToRemove = new List<string>();

        private void Awake()
        {
            var c = GetComponent<Camera>();
            if(c == null)
            {
                Debug.LogError("Missing camera");
                return;
            }
            LayerUtils.EditCameraCullingMask(c, layerNamesToAdd, layerNamesToRemove);
        }
    }
}
