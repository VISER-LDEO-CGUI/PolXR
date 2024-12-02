using UnityEngine;
using UnityEditor;

namespace Fusion.XR.Host.Desktop
{
    [CustomEditor(typeof(RigSelection))]
    public class RigSelectionEditor : UnityEditor.Editor
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
}

