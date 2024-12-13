using Fusion.XR.Shared.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Fusion.XR.Shared.Utils
{
    /**
     * Ensure that the user camera cannot see its own avatar, by making its layer "invisible"
     */
    public class HideForLocalUser : NetworkBehaviour
    {
        public string localUserLayer = "InvisibleForLocalPlayer";
        public string remoteUserLayer = "Default";
        public bool hideLocalAvatar = true;
        public bool applyLayerToChildren = true;
        HardwareRig rig;

        public bool hideOnAllPlatforms = true;
        [DrawIf(nameof(hideOnAllPlatforms), true, CompareOperator.NotEqual, Hide = true)]
        public List<RuntimePlatform> hideOnPlatforms = new List<RuntimePlatform>();

        private void Awake()
        {
            rig = FindObjectOfType<HardwareRig>();
        }

        void ConfigureCamera()
        {
            int layer = LayerMask.NameToLayer(localUserLayer);
            if (hideLocalAvatar && layer != -1)
            {
                var camera = rig.headset.GetComponentInChildren<Camera>();
                camera.cullingMask &= ~(1 << layer);
#if POLYSPATIAL_SDK_AVAILABLE
                foreach(var volumeCamera in FindObjectsOfType<Unity.PolySpatial.VolumeCamera>()){
                    volumeCamera.CullingMask &= ~(1 << layer);
                }
#endif
            }
        }

        private void Start()
        {
            // Change camera culling mask to hide local user, if required by hideLocalAvatar
            ConfigureCamera();
        }

        public bool ShouldHide()
        {
            if (hideOnAllPlatforms) return true;
            if (hideOnPlatforms.Contains(Application.platform)) return true;
            return false;
        }

        public override void Spawned()
        {
            base.Spawned();
            if (Object.HasInputAuthority && ShouldHide()) {
                ConfigureLocalRenderers();
            } 
            else
            {
                ConfigureRemoteRenderers();
            }
        }

        public void ConfigureLocalRenderers()
        {
            if (localUserLayer != "")
            {
                int layer = LayerMask.NameToLayer(localUserLayer);
                if (layer == -1)
                {
                    Debug.LogError($"Local will be visible and may obstruct you vision. Please add a {localUserLayer} layer (it will be automatically removed on the camera culling mask)");
                }
                else
                {
                    if (applyLayerToChildren)
                    {
                        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
                        {
                            renderer.gameObject.layer = layer;
                        }
                    }
                    gameObject.layer = layer;
                }
            }
        }

        public void ConfigureRemoteRenderers()
        {
            int layer = LayerMask.NameToLayer(remoteUserLayer);
            if (applyLayerToChildren)
            {
                foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
                {
                    renderer.gameObject.layer = layer;
                }
            }
            gameObject.layer = layer;
        }
    }
}
