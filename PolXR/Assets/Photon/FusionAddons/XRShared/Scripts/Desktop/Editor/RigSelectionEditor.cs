using UnityEngine;
using UnityEditor;
using Fusion.XR.Shared.Desktop;


/***
 * 
 * Add editor buttons to choose between desktop and VR rig
 * 
 **/
[CustomEditor(typeof(RigSelection))]
public class RigSelectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RigSelection rigSelection = (RigSelection)target;

        if (GUILayout.Button("Set User Preference to Desktop Rig"))
        {
            rigSelection.SetDesktopPreference();
        }

        if (GUILayout.Button("Set User Preference to VR Rig"))
        {
            rigSelection.SetVRPreference();
        }
    }
}

