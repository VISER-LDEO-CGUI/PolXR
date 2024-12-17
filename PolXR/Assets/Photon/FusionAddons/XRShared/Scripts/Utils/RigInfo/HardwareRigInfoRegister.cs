using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared.Rig
{
    /**
    * Register in a rig info a HardwareRig component on this gameobject
    *
    */
    public class HardwareRigInfoRegister : MonoBehaviour
    {
        public RigInfo rigInfo;

        private void Awake()
        {
            if (TryGetComponent(out HardwareRig hardwareRig))
            {
                if (rigInfo == null) rigInfo = RigInfo.FindRigInfo(allowSceneSearch: true);
                if (rigInfo)
                    rigInfo.RegisterHardwareRig(hardwareRig);
                else
                    Debug.LogError("HardwareRigInfoRegister cannot work without a RigInfo in the scene");
            }
        }
    }
}

