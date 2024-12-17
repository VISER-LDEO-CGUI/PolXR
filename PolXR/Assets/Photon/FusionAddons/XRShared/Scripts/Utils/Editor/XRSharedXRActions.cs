using Fusion.XR.Shared.Editor;
using Fusion.XR.Shared.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


[InitializeOnLoad]
class XRSharedXRActions
{
    struct CreateHardwareRigXRAction : IXRRootAction, IXRActionSelecter
    {
        public string Description => IsInstalled ? "<color=#A7A7A7>Select</color> hardware rig" : "<color=#A7A7A7>Create</color> hardware rig";
        public string CategoryName => XRActionsManager.HARDWARERIG_CATEGORY;
        public int Weight => 0;

        public string ImageName => "xrguide-spawn-hardwarerig";
        public bool IsInstalled
        {
            get
            {
                return XRSharedAutomation.TryGetHardwareRig(out _);
            }
        }
        public bool IsActionVisible
        {
            get
            {
                return true;
            }
        }

        public bool TrySelect()
        {
            if (IsInstalled)
            {
                XRProjectAutomation.ExitPrefabMode();
                XRSharedAutomation.TryGetHardwareRig(out var hardwareRig);
                Selection.activeObject = hardwareRig;
                return true;
            }
            return false;
        }

        public void Trigger()
        {
            if (TrySelect() == false)
            {
                XRProjectAutomation.ExitPrefabMode();
                XRSharedAutomation.CreateHardwareRig(out var hardwareRigObject);
                XRActionsManager.AddLog("Created <color=HIGHLIGHT_COLOR>[hardware rig]</color>\n- tracks the device play area", selecter: this, imageName: ImageName);
                if (hardwareRigObject.TryGetComponent<HardwareRig>(out var hardwareRig))
                {
                    XRActionsManager.AddLog("Created <color=HIGHLIGHT_COLOR>[hardware headset]</color>\n- tracks the device headset. Also offers the camera, and view fading capabilities", associatedObject: hardwareRig.headset, imageName: ImageName, forceExitPrefabMode: true);
                    XRActionsManager.AddLog("Created <color=HIGHLIGHT_COLOR>[hardware hands]</color>\n- tracks the device controllers position. Also offers grabbing capabilities", associatedObject: hardwareRig.leftHand, imageName: ImageName, forceExitPrefabMode: true);
                }
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
    struct CreateNetworkRigXRAction : IXRRootAction, IXRActionSelecter
    {
        public string Description => IsInstalled ? "<color=#A7A7A7>Edit</color> network rig" : "<color=#A7A7A7>Create</color> network rig";
        public int Weight => 0;
        public string CategoryName => XRActionsManager.NETWORKRIG_CATEGORY;
        public string ImageName => "xrguide-spawn-networkrig";

        public bool IsInstalled
        {
            get
            {
                var userSpawner = XRSharedAutomation.CurrentUserSpawner();
                return userSpawner != null && userSpawner.UserPrefab;
            }
        }
        public bool IsActionVisible
        {
            get
            {
                return true;
            }
        }

        public bool TrySelect()
        {
            if (IsInstalled)
            {
                var userSpawner = XRSharedAutomation.CurrentUserSpawner();
                Selection.activeObject = userSpawner.UserPrefab;
                string assetPath = AssetDatabase.GetAssetPath(userSpawner.UserPrefab);
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
                return true;
            }
            return false;
        }

        public void Trigger()
        {
            if (TrySelect() == false)
            {
                if(XRSharedAutomation.TryCreateNetworkRig(out var userSpawner, out var networkRigPrefab))
                {
                    XRActionsManager.AddLog("Created <color=HIGHLIGHT_COLOR>[network rig]</color>\n- contains NetworkTransforms to synchronize the position of the rig parts (head, hands, play area)", associatedObject: networkRigPrefab, imageName: ImageName);
                    XRActionsManager.AddLog("Register the network in the <color=HIGHLIGHT_COLOR>[connection manager]</color>\n- sets the network rig as the user spawned prefab upon connection</color>", associatedObject: (MonoBehaviour)userSpawner, imageName: ImageName, forceExitPrefabMode: true);
                }
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
    struct AddDummyAvatarToNetworkRigXRAction : IXRAction, IXRActionSelecter
    {
        public string Description => IsInstalled ? "Demo avatar <color=#A7A7A7>installed</color>" : "<color=#A7A7A7>Add</color> demo avatar";
        public string CategoryName => XRActionsManager.NETWORKRIG_CATEGORY;
        public string ImageName => "xrguide-add-dummyavatar-networkrig";
        public int Weight => 100;
        public bool IsInstalled
        {
            get
            {
                var userSpawner = XRSharedAutomation.CurrentUserSpawner();
                if (userSpawner == null || userSpawner.UserPrefab == null)
                {
                    return false;
                }


                bool found = false;
                if (XRProjectAutomation.TryFindPrefabInstanceInChildren(userSpawner.UserPrefab.gameObject, XRSharedAutomation.dummyAvatarGuid, out _))
                {
                    found = true;
                }

                return found;

            }
        }
        public bool IsActionVisible
        {
            get
            {
                return true;
            }
        }

        public bool TrySelect()
        {
            if (IsInstalled)
            {
                var userSpawner = XRSharedAutomation.CurrentUserSpawner();
                string assetPath = AssetDatabase.GetAssetPath(userSpawner.UserPrefab);
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
                var stage = PrefabStageUtility.GetCurrentPrefabStage();

                var networkHeadset = stage.FindComponentOfType<NetworkHeadset>();
                if (networkHeadset && XRProjectAutomation.TryFindPrefabInstanceInChildren(networkHeadset.gameObject, XRSharedAutomation.dummyAvatarGuid, out var avatar))
                {
                    Selection.activeObject = avatar;
                }
                return true;
            }
            return false;
        }

        public void Trigger()
        {
            if (TrySelect() == false)
            {
                XRSharedAutomation.AddDummyAvatarToNetworkRigHeadset(out var includesHideForLocalUser);
                XRSharedAutomation.AddOSFHandsToNetworkHands();
                string note = "";
                if (includesHideForLocalUser)
                    note = "\n- <color=ALERT_COLOR>Note: add a InvisibleForLocalPlayer layer in your project to hide the avatar head to the local user</color>";
                XRActionsManager.AddLog("Created <color=HIGHLIGHT_COLOR>[demo avatar]</color>" + note, selecter: this, imageName: ImageName);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

    }
    struct CreateGrabbableXRAction : IXRAction
    {
        public string Description => "<color=#A7A7A7>Add</color> grabbable object";
        public string CategoryName => XRActionsManager.SCENE_OBJECT_CATEGORY;
        public int Weight => 0;

        public string ImageName => "xrguide-add-grabbable-object";

        public bool IsActionVisible
        {
            get
            {
                return true;
            }
        }
        public void Trigger()
        {
            XRProjectAutomation.ExitPrefabMode();
            var o = XRSharedAutomation.CreateGrabbableObject();

            XRActionsManager.AddLog("Create <color=HIGHLIGHT_COLOR>[grabbable]</color>", associatedObject: o, imageName: ImageName, forceExitPrefabMode: true);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
    struct CreateLaunchableGrabbableXRAction : IXRAction
    {
        public string Description =>  "<color=#A7A7A7>Add</color> launchable object";
        public string CategoryName => XRActionsManager.SCENE_OBJECT_CATEGORY;
        public int Weight => 0;

        public string ImageName => "xrguide-add-launchable-object";

        public bool IsActionVisible
        {
            get
            {
                return true;
            }
        }
        public void Trigger()
        {
            XRProjectAutomation.ExitPrefabMode();
            var o = XRSharedAutomation.CreateLaunchableObject();

            XRActionsManager.AddLog("Create <color=HIGHLIGHT_COLOR>[launchable grabbable]</color>", associatedObject: o, imageName: ImageName, forceExitPrefabMode: true);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    static CreateHardwareRigXRAction createHardwareRigXRAction;
    static CreateNetworkRigXRAction createNetworkRigXRAction;
    static AddDummyAvatarToNetworkRigXRAction addDummyAvatarToNetworkRigXRAction;
    static CreateGrabbableXRAction createGrabbableXRAction;
    static CreateLaunchableGrabbableXRAction createLaunchableGrabbableXRAction;


    static XRSharedXRActions()
    {
        XRActionsManager.RegisterAction(createHardwareRigXRAction);
        XRActionsManager.RegisterAction(createNetworkRigXRAction);
        XRActionsManager.RegisterAction(addDummyAvatarToNetworkRigXRAction);
        XRActionsManager.RegisterAction(createGrabbableXRAction);
        XRActionsManager.RegisterAction(createLaunchableGrabbableXRAction);
    }
}
