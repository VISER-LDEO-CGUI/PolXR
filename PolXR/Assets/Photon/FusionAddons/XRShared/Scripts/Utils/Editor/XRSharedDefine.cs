#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class XRSharedDefine
{
    const string DEFINE = "XRSHARED_ADDON_AVAILABLE"; 

    static XRSharedDefine()
    {
        var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        // Meta core
        if (defines.Contains(DEFINE) == false)
        {
            defines = $"{defines};{DEFINE}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
    }
}
#endif