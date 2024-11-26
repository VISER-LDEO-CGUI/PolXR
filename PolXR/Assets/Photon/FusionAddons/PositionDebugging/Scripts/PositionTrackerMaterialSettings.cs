using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PositionTrackerMaterialSettings", menuName = "Fusion Addons/PositionTrackerMaterialSettings", order = 1)]
public class PositionTrackerMaterialSettings : ScriptableObject
{

    [System.Serializable]
    public struct MaterialSettings
    {
        public Material lineMaterial;
        public Material primitiveMaterial;
    }

    [Header("Default material settings")]
    public MaterialSettings defaultMaterialSettings;
    [Header("Phase specific material settings")]
    public MaterialSettings renderMaterialSettings;
    public MaterialSettings funForwardMaterialSettings;
    public MaterialSettings funResimMaterialSettings;
    public MaterialSettings funFirstResimMaterialSettings;
    public MaterialSettings lateUpdateMaterialSettings;
    public MaterialSettings fixedUpdateMaterialSettings;
    [Header("Velocity visualization material settings")]
    public Material velocityChangeMaterial;

    public static PositionTrackerMaterialSettings DefaultSettings()
    {
        var materialSettingsAsset = Resources.Load<PositionTrackerMaterialSettings>("DefaultPositionTrackerMaterialSettings");
        return materialSettingsAsset;
    }
}
