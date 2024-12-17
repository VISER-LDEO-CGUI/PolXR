using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Samples.IndustriesComponents
{
    public class DeleteForDesktop : MonoBehaviour
    {
        void OnEnable()
        {
#if !UNITY_ANDROID && !UNITY_EDITOR
        DestroyImmediate( gameObject );
#endif
        }
    }
}

