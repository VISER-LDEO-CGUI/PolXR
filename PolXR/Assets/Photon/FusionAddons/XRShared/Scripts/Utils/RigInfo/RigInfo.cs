using Fusion.XR.Shared.Desktop;
using Fusion.XR.Shared.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared.Rig
{
    
    /***
     * 
     * RigInfo registers & centralizes information about HardwareRig & NetworkRig.
     * In this way, other classes can easily retrieve this information. 
     * 
     ***/
    public class RigInfo : MonoBehaviour
    {
        public enum RigKind
        {
            Undefined,
            VR,
            Desktop
        }

        public RigKind localHardwareRigKind = RigKind.Undefined;
        public DesktopController localHardwareRigDesktopController = null;

        [Header("Local rigs")]
        public HardwareRig localHardwareRig;
        public NetworkRig localNetworkedRig;

        public void RegisterNetworkRig(NetworkRig networkRig)
        {
            localNetworkedRig = networkRig;
        }

        public void RegisterHardwareRig(HardwareRig hardwareRig)
        {
            localHardwareRig = hardwareRig;
            if (hardwareRig)
            {
                localHardwareRigDesktopController = hardwareRig.GetComponentInChildren<DesktopController>();
                if (localHardwareRigDesktopController)
                {
                    localHardwareRigKind = RigKind.Desktop;
                } else
                {
                    localHardwareRigKind = RigKind.VR;
                }
            }
        }
      

        /**
         * Look for a RigInfo, under the runner hierarchy
         */
        public static RigInfo FindRigInfo(NetworkRunner runner = null, bool allowSceneSearch = false)
        {
            RigInfo rigInfo = null;
            if (runner != null) rigInfo = runner.GetComponentInChildren<RigInfo>();
            if (rigInfo == null && allowSceneSearch) rigInfo = FindObjectOfType<RigInfo>(true);
            if (rigInfo == null)
            {
                Debug.LogWarning("Unable to find RigInfo: it should be stored under the runner hierarchy");
            }
            return rigInfo;
        }
    }
}

