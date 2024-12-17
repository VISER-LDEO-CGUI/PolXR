using Fusion.XR.Shared.Grabbing;
using Fusion.XR.Shared.Rig;
using Fusion.XR.Shared.SimpleHands;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fusion.XR.Shared.Editor
{
    public static class XRProjectAutomation
    {
        const bool useRootFolder = true;
        public static string RootFolder = "XRProject";
        public static bool TryGetPrefabsFolderPath(out string path)
        {
            const string prefabFolder = "Prefabs";
            var basePath = "Assets";
            if (useRootFolder && TryGetFolderPath("Assets", RootFolder, out basePath) == false)
            {
                path = null;
                return false;
            }
            return TryGetFolderPath(basePath, prefabFolder, out path);
        }

        public static bool TryGetFolderPath(string parentPath, string folderName, out string path)
        {
            path = $"{parentPath}/{folderName}";
            if (AssetDatabase.IsValidFolder(parentPath) == false)
            {
                Debug.LogError($"Parent path {parentPath} not valid");
                return false;
            }
            if (AssetDatabase.IsValidFolder(path) == false)
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
                if (AssetDatabase.IsValidFolder(path) == false)
                {
                    Debug.LogError($"Unable to create {path} folder");
                    return false;
                }
            }

            return true;
        }

        public static bool TryFindPrefabInstanceInChildren(GameObject currentObject, string guid, out GameObject prefabInstance)
        {
            prefabInstance = null;
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            string instancePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(currentObject);
            if (instancePath == prefabPath)
            {
                prefabInstance = currentObject;
                return true;
            }

            foreach (Transform child in currentObject.transform)
            {
                bool found = TryFindPrefabInstanceInChildren(child.gameObject, guid, out prefabInstance);
                if (found)
                    return true;
            }
            return false;
        }

        public static void AddAsChild(Component parent, GameObject child)
        {
            child.transform.parent = parent.transform;
            child.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public static void AddAsChild(GameObject parent, GameObject child)
        {
            child.transform.parent = parent.transform;
            child.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public static GameObject SpawnPrefab(string guid)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        public static bool SpawnChildIfnotPresent(GameObject parent, string guid, out GameObject child)
        {
            if (XRProjectAutomation.TryFindPrefabInstanceInChildren(parent, guid, out child) == false)
            {
                child = XRProjectAutomation.SpawnPrefab(guid);
                XRProjectAutomation.AddAsChild(parent: parent, child: child);
                return true;
            }
            return false;
        }
        public static bool SpawnChildIfnotPresent(Component parent, string guid, out GameObject child)
        {
            if (XRProjectAutomation.TryFindPrefabInstanceInChildren(parent.gameObject, guid, out child) == false)
            {
                child = XRProjectAutomation.SpawnPrefab(guid);
                XRProjectAutomation.AddAsChild(parent: parent, child: child);
                return true;
            }
            return false;
        }

        public static bool SpawnChildIfnotPresent(GameObject parent, string guid)
        {
            return SpawnChildIfnotPresent(parent, guid, out _);
        }

        public static bool SpawnChildIfnotPresent(Component parent, string guid)
        {
            return SpawnChildIfnotPresent(parent, guid, out _);
        }

        public static void ExitPrefabMode()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                StageUtility.GoBackToPreviousStage();
            };
        }
    }

}


