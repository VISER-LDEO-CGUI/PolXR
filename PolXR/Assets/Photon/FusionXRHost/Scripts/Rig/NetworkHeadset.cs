using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Rig
{
    /**
     * 
     * Network VR user headset
     * Position synchronization is handled in the NetworkRig
     * 
     **/

    [RequireComponent(typeof(NetworkTransform))]
    public class NetworkHeadset : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkTransform networkTransform;

        private void Awake()
        {
            if (networkTransform == null) networkTransform = GetComponent<NetworkTransform>();
        }
    }
}

