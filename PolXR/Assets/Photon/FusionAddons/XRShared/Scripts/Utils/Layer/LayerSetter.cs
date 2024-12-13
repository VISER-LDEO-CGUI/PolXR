using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared.Utils
{
    /*
     * Automaticaly set a layer, or warn the dev that it does not exists yet
     */
    public class LayerSetter : MonoBehaviour
    {
        public string layerToApplyName = "RequiredLayerName";
        public bool applyLayerToChildren = false;

        private void Awake()
        {
            ApplyLayer();
        }
        
        public virtual void ApplyLayer()
        {
            LayerUtils.ApplyLayer(gameObject, layerToApplyName, applyLayerToChildren);
        }
    }
}
