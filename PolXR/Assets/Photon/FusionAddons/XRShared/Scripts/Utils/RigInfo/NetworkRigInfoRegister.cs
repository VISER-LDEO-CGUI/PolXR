using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared.Rig
{
    /**
     * Register in a rig info a NetworkRig component on this gameobject
     * It should not be placed on a NetworkRig gameobject that are not associated with a player (Bots, ...)
     */
    public class NetworkRigInfoRegister : NetworkBehaviour
    {
        public RigInfo rigInfo;

        public override void Spawned()
        {
            base.Spawned();
            if (TryGetComponent(out NetworkRig networkRig))
            {
                if (Object.HasInputAuthority)
                {
                    if (rigInfo == null) rigInfo = RigInfo.FindRigInfo(Runner);
                    if(rigInfo) 
                        rigInfo.RegisterNetworkRig(networkRig);
                    else
                        Debug.LogError("NetworkRigInfoRegister cannot work without a RigInfo in the scene");
                }
            }
        }
    }
}
