// Comment if you do not use the physics addon
#define USE_PHYSICSADDON
#if USE_PHYSICSADDON
using Fusion.Addons.Physics;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Addons.PositionDebugging
{
    /**
     * Created line renderer and primitives to visual display the position of this objects's transform during certain phases.
     * 
     * If hideLinesAtCreation/hidePrimitivesAtCreation are true at start, in the inspector
     * use "Toggle display line" / "Toggle display primitives" buttons
     * 
     */
    [DefaultExecutionOrder (PositionTracker.EXECUTION_ORDER)]
    public class PositionTracker : NetworkBehaviour
    {
        public const int EXECUTION_ORDER = 10_000;
        Dictionary<string, DebugRoot> roots = new Dictionary<string, DebugRoot>();

        public bool hideLinesAtCreation = true;
        public bool hidePrimitivesAtCreation = true;

        [System.Flags]
        public enum LoggedState
        {
            None = 0,
            FUNForward = 1,
            FUNResim = 2,
            Render = 4,
#if USE_PHYSICSADDON
            RenderInterpolationTarget = 8,
#endif
            FUNFirstResim = 32,
            FUN = 64,
            LateUpdate = 128,
#if USE_PHYSICSADDON
            LateUpdateInterpolationTarget = 256,
#endif
            FixedUpdate = 512,
#if USE_PHYSICSADDON
            FixedUpdateInterpolationTarget = 1024,
#endif

        }

        [Header("Log Selection For Each Type of Client")]
        public LoggedState loggedStatesForAllClients = LoggedState.None;
        public LoggedState loggedStatesForStateAuthorityOnly = LoggedState.None;
        public LoggedState loggedStatesForInputAuthorityOnly = LoggedState.None;
        public LoggedState loggedStatesForProxyOnly = LoggedState.None;

        [Header("Material settings")]
        public PositionTrackerMaterialSettings materialSettings;

        PositionTrackerMaterialSettings.MaterialSettings DefaultMaterialSettings => materialSettings != null ? materialSettings.defaultMaterialSettings : default;
        PositionTrackerMaterialSettings.MaterialSettings RenderMaterialSettings => materialSettings != null ? materialSettings.renderMaterialSettings : default;
        PositionTrackerMaterialSettings.MaterialSettings FunForwardMaterialSettings => materialSettings != null ? materialSettings.funForwardMaterialSettings : default;
        PositionTrackerMaterialSettings.MaterialSettings FunResimMaterialSettings => materialSettings != null ? materialSettings.funResimMaterialSettings : default;
        PositionTrackerMaterialSettings.MaterialSettings FunFirstResimMaterialSettings => materialSettings != null ? materialSettings.funFirstResimMaterialSettings : default;
        PositionTrackerMaterialSettings.MaterialSettings LateUpdateMaterialSettings => materialSettings != null ? materialSettings.lateUpdateMaterialSettings : default;
        PositionTrackerMaterialSettings.MaterialSettings FixedUpdateMaterialSettings => materialSettings != null ? materialSettings.fixedUpdateMaterialSettings : default;

        [Tooltip("Display a prefab instead of basic primitive. Can be very resource intensive, use with care.")]
        public GameObject primitivePrefab = null;


        [Header("Debug representation")]
        public float scale = 0.001f;
        public bool debugRotation = false;
        public interface IPositionTrackerExtension
        {
            DebugRoot.StateInfo StateInfoState(LoggedState state);
            Material MetadataMaterial(int md);
        }
        public IPositionTrackerExtension positionTrackerExtension;

        public void SelectMaterial(PositionTrackerMaterialSettings.MaterialSettings settings, out Material lineMaterial, out Material primitiveMaterial)
        {
            lineMaterial = settings.lineMaterial != null ? settings.lineMaterial : DefaultMaterialSettings.lineMaterial;

            primitiveMaterial = settings.primitiveMaterial != null ? settings.primitiveMaterial : DefaultMaterialSettings.primitiveMaterial;
        }

#if USE_PHYSICSADDON
        NetworkRigidbody3D nrb;
#endif

        private void Awake()
        {
#if USE_PHYSICSADDON
            nrb = GetComponent<NetworkRigidbody3D>();
#endif
            if (positionTrackerExtension == null) positionTrackerExtension = GetComponent<IPositionTrackerExtension>();
        }

        private void Start()
        {
            if (materialSettings == null)
            {
                materialSettings = PositionTrackerMaterialSettings.DefaultSettings();
            }
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        bool ShouldLogState(LoggedState phase)
        {
            if (Object && (loggedStatesForAllClients & phase) != 0)
            { 
                return true;
            }
            else if (Object && Object.HasStateAuthority)
            {
                return (loggedStatesForStateAuthorityOnly & phase) != 0;
            }
            else if (Object && Object.HasInputAuthority)
            {
                return (loggedStatesForInputAuthorityOnly & phase) != 0;
            }
            else if (Object && Object.HasStateAuthority == false && Object.HasInputAuthority == false)
            {
                return (loggedStatesForProxyOnly & phase) != 0;
            }
            return false;

        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (ShouldLogState(LoggedState.FUN))
            {
                if (Runner.IsForward)
                {
                    DebugRoot.StateInfo info = InfoForState(LoggedState.FUN, $"[Forward] {Runner.Tick}");
                    SelectMaterial(FunForwardMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                    CreateDebugPoint(transform, PrimitiveType.Cube, info, $"FUN-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
                }
                else
                {
                    DebugRoot.StateInfo info = InfoForState(LoggedState.FUN, $"{((Runner.IsFirstTick) ? "(FirstResim)" : "(Resim)")}{Runner.Tick}");
                    SelectMaterial(FunResimMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                    CreateDebugPoint(transform, PrimitiveType.Cylinder, info, $"FUN-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
                }
            }

            if (Runner.IsForward)
            {
                if (ShouldLogState(LoggedState.FUNForward))
                {
                    DebugRoot.StateInfo info = InfoForState(LoggedState.FUN, $"[Forward] {Runner.Tick}");
                    SelectMaterial(FunForwardMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                    CreateDebugPoint(transform, PrimitiveType.Cube, info, $"(Forward)FUN-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
                }
            }
            else
            {
                if (Runner.IsFirstTick && ShouldLogState(LoggedState.FUNFirstResim))
                {
                    DebugRoot.StateInfo info = InfoForState(LoggedState.FUN, $"[FirstResim] {Runner.Tick}");
                    SelectMaterial(FunFirstResimMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                    CreateDebugPoint(transform, PrimitiveType.Cylinder, info, $"(FirstResim)-FUN-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
                }
                if (ShouldLogState(LoggedState.FUNResim))
                {
                    DebugRoot.StateInfo info = InfoForState(LoggedState.FUN, $"{((Runner.IsFirstTick) ? "(FirstResim)" : "(Resim)")}{Runner.Tick}");
                    SelectMaterial(FunResimMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                    CreateDebugPoint(transform, PrimitiveType.Cylinder, info, $"(Resim)FUN-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
                }
            }
        }

        public override void Render()
        {
            base.Render();
            if (ShouldLogState(LoggedState.Render))
            {
                DebugRoot.StateInfo info = InfoForState(LoggedState.Render, $"{Time.time}");
                SelectMaterial(RenderMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                CreateDebugPoint(transform, PrimitiveType.Sphere, info, $"Render-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
            }
#if USE_PHYSICSADDON
            if (ShouldLogState(LoggedState.RenderInterpolationTarget) && nrb && nrb.InterpolationTarget)
            {
                DebugRoot.StateInfo info = InfoForState(LoggedState.RenderInterpolationTarget, $"{Time.time}");
                SelectMaterial(RenderMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                CreateDebugPoint(nrb.InterpolationTarget, PrimitiveType.Sphere, info, $"RenderIT-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
            }
#endif
        }

        public void LateUpdate()
        {
            if (ShouldLogState(LoggedState.LateUpdate))
            {
                DebugRoot.StateInfo info = InfoForState(LoggedState.LateUpdate, $"{Time.time}");
                SelectMaterial(LateUpdateMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                CreateDebugPoint(transform, PrimitiveType.Sphere, info, $"LateUpdate-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
            }
#if USE_PHYSICSADDON
            if (ShouldLogState(LoggedState.LateUpdateInterpolationTarget) && nrb && nrb.InterpolationTarget)
            {
                DebugRoot.StateInfo info = InfoForState(LoggedState.LateUpdateInterpolationTarget, $"{Time.time}");
                SelectMaterial(LateUpdateMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                CreateDebugPoint(nrb.InterpolationTarget, PrimitiveType.Sphere, info, $"LateUpdateIT-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
            }
#endif
        }

        public void FixedUpdate()
        {
            if (ShouldLogState(LoggedState.FixedUpdate))
            {
                DebugRoot.StateInfo info = InfoForState(LoggedState.FixedUpdate, $"{Time.time}");
                SelectMaterial(FixedUpdateMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                CreateDebugPoint(transform, PrimitiveType.Sphere, info, $"FixedUpdate-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
            }
#if USE_PHYSICSADDON
            if (ShouldLogState(LoggedState.FixedUpdateInterpolationTarget) && nrb && nrb.InterpolationTarget)
            {
                DebugRoot.StateInfo info = InfoForState(LoggedState.FixedUpdateInterpolationTarget, $"{Time.time}");
                SelectMaterial(FixedUpdateMaterialSettings, out var lineMaterial, out var primitiveMaterial);
                CreateDebugPoint(nrb.InterpolationTarget, PrimitiveType.Sphere, info, $"FixedUpdateIT-{name}-{Object.Id}", lineMaterial, primitiveMaterial);
            }
#endif
        }

        DebugRoot.StateInfo InfoForState(LoggedState state, string defaultDescription)
        {
            DebugRoot.StateInfo info;
            info.md = -1;
            info.name = defaultDescription;
            info.rotation = Quaternion.identity;
            if (positionTrackerExtension != null)
            {
                info = positionTrackerExtension.StateInfoState(state);
            }
            return info;
        }

        void CreateDebugPoint(Transform t, PrimitiveType type, DebugRoot.StateInfo info, string rootName, Material lineMaterial, Material primitiveMaterial)
        {
            var root = DebugRoot.Find(roots, rootName, lineMaterial, primitiveMaterial, hideLinesAtCreation, hidePrimitivesAtCreation);
            root.scale = scale;
            if(primitivePrefab) root.primitivePrefab = primitivePrefab;
            if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root.gameObject, gameObject.scene);
            }
            if (info.md != -1 && positionTrackerExtension != null)
            {
                if (root.mdMaterials.ContainsKey(info.md) == false)
                {
                    var material = positionTrackerExtension.MetadataMaterial(info.md);
                    root.mdMaterials[info.md] = material;
                }
            }
            if (debugRotation)
            {
                info.rotation = transform.rotation;
            }
            root.primitiveType = type;
            root.AddPoint(t.position, info);
        }

        [EditorButton("Toggle display line")]
        public void ToggleDisplayLine()
        {
            foreach (var root in roots.Values) root.ToggleDisplayLine();
        }

        [EditorButton("Toggle display primitives")]
        public void ToggleDisplayPrimitives()
        {
            foreach (var root in roots.Values) root.ToggleDisplayPrimitives();
        }


        [EditorButton("Reset points")]
        public void ResetPoints()
        {
            foreach (var root in roots.Values) root.ResetPoints();

        }
    }
}
