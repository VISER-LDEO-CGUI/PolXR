#if XRSHARED_ADDON_AVAILABLE
using Fusion.XR.Shared.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
class ConnectionManagerXRActions
{
    struct CreateRunnerXRAction : IXRRootAction, IXRActionSelecter
    {
        public string Description => IsInstalled ? "<color=#A7A7A7>Select</color> runner" : "<color=#A7A7A7>Create</color> runner";
        public string CategoryName => XRActionsManager.RUNNER_CATEGORY;
        public int Weight => 0;

        public string ImageName => "xrguide-spawn-runner";
        public bool IsInstalled
        {
            get
            {
                return XRSharedAutomation.CurrentUserSpawner() != null;
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
                Selection.activeObject = XRSharedAutomation.CurrentUserSpawner() as UnityEngine.Object;
                return true;
            }
            return false;
        }

        public void Trigger()
        {
            if (TrySelect() == false)
            {
                XRProjectAutomation.ExitPrefabMode();
                ConnectionManagerAutomation.CreateConnectionManager();
                XRActionsManager.AddLog("Created <color=HIGHLIGHT_COLOR>[network runner]</color>\n- handles Fusion session", selecter: this, imageName: ImageName);
                XRActionsManager.AddLog("Created <color=HIGHLIGHT_COLOR>[connection manager]</color>\n- handles user prefab spawn and room selection", selecter: this, imageName: ImageName);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

    }

    static CreateRunnerXRAction createRunnerXRAction;

    static ConnectionManagerXRActions()
    {
        XRActionsManager.RegisterAction(createRunnerXRAction);
    }
}
#endif

