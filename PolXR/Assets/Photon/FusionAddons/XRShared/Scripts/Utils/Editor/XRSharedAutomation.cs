using Fusion.Addons.Physics;
using Fusion.XR.Shared.Grabbing;
using Fusion.XR.Shared.Rig;
using Fusion.XR.Shared.SimpleHands;
using Fusion.XR.Shared.Utils;
using UnityEditor;
using UnityEngine;

namespace Fusion.XR.Shared.Editor
{
    public static class XRSharedAutomation
    {
        public const string leftOSFHandGuid = "814fa524bf762ee4fafde3bb2f561611";
        public const string rightOSFHandGuid = "bbe1b25275c702c4a80f203311c3eed2";
        const string hardwareRigGuid = "72aacf5ad018ef84f8c2f1796da09345";
        public const string dummyAvatarGuid = "8a423e339629e4f1a8bdb2b8bb5e0b2d";
        public const string networkGrabbableGuid = "71813bede6cf5634eb4bfd5f3ffd28e7";
        public const string networkLaunchableGuid = "116031ca311ff43c49111c6a44cccdd4";

        public static IUserSpawner CurrentUserSpawner()
        {
            var runner = GameObject.FindObjectOfType<NetworkRunner>();
            if (runner == null) return null;

            var userSpawner = runner.GetComponentInChildren<IUserSpawner>();
            return userSpawner;
        }

        public static string CurrentUserSpawnerPrefabPath()
        {
            var userSpawner = XRSharedAutomation.CurrentUserSpawner();
            if (userSpawner == null || userSpawner.UserPrefab == null)
            {
                return null;
            }

            return AssetDatabase.GetAssetPath(userSpawner.UserPrefab);
        }

        public static bool TryCreateNetworkRig(out IUserSpawner userSpawner, out GameObject networkRigPrefab)
        {
            networkRigPrefab = null;
            userSpawner = CurrentUserSpawner();
            if (userSpawner == null)
            {
                Debug.LogError("Add a connection manager to the scene first");
                return false;
            }
            if (userSpawner.UserPrefab)
            {
                string existingPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(userSpawner.UserPrefab);
                Debug.LogError($"A NetworkRig is already configured on the ConnectionManager: {existingPath}");
                return false;
            }

            if (XRProjectAutomation.TryGetPrefabsFolderPath(out var prefabsPath) == false)
            {
                return false;
            }

            const string networkRigGuid = "477f02bca0b59924aa984e240cb28cc4";
            var prefabPath = AssetDatabase.GUIDToAssetPath(networkRigGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject variantDraft = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            GameObject prefabObject = PrefabUtility.SaveAsPrefabAsset(variantDraft, $"{prefabsPath}/NetworkRigVariant.prefab");
            GameObject.DestroyImmediate(variantDraft);

            if (prefabObject.TryGetComponent<NetworkObject>(out var no))
            {
                networkRigPrefab = prefabObject;
                userSpawner.UserPrefab = no;
                PrefabUtility.RecordPrefabInstancePropertyModifications((Object)userSpawner);
            }
            else
            {
                Debug.LogError("Network rig prefab issue.");
            }
            return true;
        }


        public static bool TryGetHardwareRig(out HardwareRig hardwareRig)
        {
            hardwareRig = null;
            if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent<HardwareRig>(out hardwareRig))
            {

            }
            else
            {
                var hardwareRigs = GameObject.FindObjectsOfType<HardwareRig>();
                if (hardwareRigs.Length == 0)
                {
                    return false;
                }
                else if (hardwareRigs.Length != 1)
                {
                    Debug.LogError("Several hardware rig: select one");
                    return false;
                }
                else
                {
                    hardwareRig = hardwareRigs[0];
                }
            }
            return true;
        }

        public static bool TryGetGrabbableObject(out NetworkGrabbable networkGrabbable)
        {
            networkGrabbable = null;
            if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent<NetworkGrabbable>(out networkGrabbable))
            {

            }
            else
            {
                var networkGrabbables = GameObject.FindObjectsOfType<NetworkGrabbable>();
                if (networkGrabbables.Length == 0)
                {
                    return false;
                }
                else if (networkGrabbables.Length != 1)
                {
                    Debug.LogError("Several networkGrabbable: select one");
                    return false;
                }
                else
                {
                    networkGrabbable = networkGrabbables[0];
                }
            }
            return true;
        }

        public static bool TryGetLaunchableObject(out NetworkRigidbody3D networkRigidbody3D)
        {
            networkRigidbody3D = null;
            if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent<NetworkRigidbody3D>(out networkRigidbody3D))
            {

            }
            else
            {
                var networkRigidbody3Ds = GameObject.FindObjectsOfType<NetworkRigidbody3D>();
                if (networkRigidbody3Ds.Length == 0)
                {
                    return false;
                }
                else if (networkRigidbody3Ds.Length != 1)
                {
                    Debug.LogError("Several NetworkRigidbody3D: select one");
                    return false;
                }
                else
                {
                    networkRigidbody3D = networkRigidbody3Ds[0];
                }
            }
            return true;
        }


        public static void CreateHardwareRig(out GameObject hardwareRig)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(hardwareRigGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            hardwareRig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            hardwareRig.transform.SetAsLastSibling();
        }

        public static GameObject CreateGrabbableObject()
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(networkGrabbableGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject networkGrabbable = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            networkGrabbable.transform.SetAsLastSibling();
            return networkGrabbable;
        }

        public static GameObject CreateLaunchableObject()
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(networkLaunchableGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject networkLaunchable = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            networkLaunchable.transform.SetAsLastSibling();
            return networkLaunchable;
        }        

        public static void AddOSFHandstoHardwareHands()
        {
            if (TryGetHardwareRig(out var hardwareRig) == false)
            {
                return;
            }

            const string leftOSFHandGuid = "814fa524bf762ee4fafde3bb2f561611";
            const string rightOSFHandGuid = "bbe1b25275c702c4a80f203311c3eed2";
            XRProjectAutomation.SpawnChildIfnotPresent(hardwareRig.leftHand, leftOSFHandGuid);
            XRProjectAutomation.SpawnChildIfnotPresent(hardwareRig.rightHand, rightOSFHandGuid);
        }

        public static void AddOSFHandsToNetworkHands()
        {
            string assetPath = XRSharedAutomation.CurrentUserSpawnerPrefabPath();
            if (assetPath == null)
            {
                Debug.LogError("Add a connection manager and a network rig set in it to the scene first");
                return;
            }

            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
            NetworkRig networkRig = contentsRoot.GetComponent<NetworkRig>();

            if (networkRig.leftHand.GetComponentInChildren<OSFHandRepresentation>() == null)
            {
                XRProjectAutomation.SpawnChildIfnotPresent(networkRig.leftHand, leftOSFHandGuid);
            }
            if (networkRig.rightHand.GetComponentInChildren<OSFHandRepresentation>() == null)
            {
                XRProjectAutomation.SpawnChildIfnotPresent(networkRig.rightHand, rightOSFHandGuid);
            }

            PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
        }

        public static void AddDummyAvatarToNetworkRigHeadset(out bool includesHideForLocaluser)
        {
            includesHideForLocaluser = false;
            string assetPath = XRSharedAutomation.CurrentUserSpawnerPrefabPath();
            if (assetPath == null)
            {
                Debug.LogError("Add a connection manager and a network rig set in it to the scene first");
                return;
            }

            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
            NetworkRig networkRig = contentsRoot.GetComponent<NetworkRig>();

            XRProjectAutomation.SpawnChildIfnotPresent(networkRig.headset, dummyAvatarGuid, out var avatar);

            includesHideForLocaluser = avatar.GetComponentInChildren<HideForLocalUser>();

            PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
        }
    }
}
