using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Samples.IndustriesComponents
{
    public class DeleteForWebGL : MonoBehaviour
    {
        void OnEnable()
        {
#if UNITY_WEBGL
        DestroyImmediate( gameObject );
#endif
        }
    }
}
