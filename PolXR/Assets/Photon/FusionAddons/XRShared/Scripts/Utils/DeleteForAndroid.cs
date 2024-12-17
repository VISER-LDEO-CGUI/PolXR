using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Samples.IndustriesComponents
{
    public class DeleteForAndroid : MonoBehaviour
    {
        void OnEnable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        Destroy(this.gameObject);
        return;
#endif
        }
    }
}
