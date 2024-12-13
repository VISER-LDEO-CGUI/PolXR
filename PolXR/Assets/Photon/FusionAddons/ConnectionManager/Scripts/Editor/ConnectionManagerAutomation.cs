#if XRSHARED_ADDON_AVAILABLE
using Fusion.XR.Shared.Rig;
using Fusion.XR.Shared.SimpleHands;
using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fusion.XR.Shared.Editor
{
    public static class ConnectionManagerAutomation
    {
        public const string connectionManagerGuid = "61132d80baa3c024d995c0e48ddf27df";

        public static void CreateConnectionManager()
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(connectionManagerGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject connectionManager = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            connectionManager.transform.SetAsLastSibling();
        }
    }
}
#endif