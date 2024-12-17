using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR.Shared
{
    /**
     * Observe any renderer on the same game object to know if it is visible or not. 
     * Useful for optimizations, espacially to know if a LOD is activated or not.
     */
    public class RendererVisible : MonoBehaviour
    {
        public bool isVisible = false;
        public UnityEvent<bool> onVisibleChange = new UnityEvent<bool>();
        public bool forceVisibleInEditor = true;

        private void Start()
        {
#if UNITY_EDITOR
            //In editor, Visible/invisible are trigerred by the scene too, invalidating the behaviour
            if (forceVisibleInEditor) isVisible = true;
#endif
        }
        private void OnBecameVisible()
        {
            isVisible = true;
            if (onVisibleChange != null) onVisibleChange.Invoke(isVisible);
        }

        private void OnBecameInvisible()
        {
            isVisible = false;
#if UNITY_EDITOR
            //In editor, Visible/invisible are trigerred by the scene too, invalidating the behaviour
            if (forceVisibleInEditor) isVisible = true;
#endif
            if (onVisibleChange != null) onVisibleChange.Invoke(isVisible);
        }
    }
}
