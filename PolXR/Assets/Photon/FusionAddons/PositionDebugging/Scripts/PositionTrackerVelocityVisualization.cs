#define USE_PHYSICSADDON
#if USE_PHYSICSADDON
using Fusion.Addons.Physics;
#endif
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Fusion.Addons.PositionDebugging
{
    /**
     * The PositionTracker data points will be colorized to match velocity gap towards the average velocity
     */
    [RequireComponent (typeof(PositionTracker))]
    class PositionTrackerVelocityVisualization : NetworkBehaviour, PositionTracker.IPositionTrackerExtension
    {

#if USE_PHYSICSADDON
        NetworkRigidbody3D nrb;
#endif

        [Header("Material settings")]
        public PositionTrackerMaterialSettings materialSettings;

        [Header("Render velocity debug")]
        public bool debugRenderVelocity = true;

        public struct VelocityAverageInfo
        {
            public float lastRenderTime;
            public Vector3 lastPos;

            public float magnitudeAverage;
            public float magnitudeTotal;
            public float magnitudeCount;


            // Local average buffer
            public const int BUFFER_SIZE = 30;
            public int ringBufferCursor;
            public float[] velBuffer;

            public static VelocityAverageInfo InitialState()
            {
                var info = new VelocityAverageInfo();
                info.lastRenderTime = -1;
                info.lastPos = Vector3.zero;

                info.magnitudeAverage = 0;
                info.magnitudeTotal = 0;
                info.magnitudeCount = 0;


                info.ringBufferCursor = 0;
                info.velBuffer = new float[BUFFER_SIZE];
                return info;
            }
        }

        public bool useRenderMaterialUnderAcceptedGap = false;
        public float gapSteps = 0.05f;
        public float acceptedGap = 0.1f;
        public float maxError = 1;
        Material velocityChangeMaterial;
        public bool useGlobalAverageVelocity = false;
        Dictionary<PositionTracker.LoggedState, VelocityAverageInfo> velocityAverageInfoByState = new Dictionary<PositionTracker.LoggedState, VelocityAverageInfo>();

        PositionTracker tracker;

        private void Awake()
        {
#if USE_PHYSICSADDON
            nrb = GetComponent<NetworkRigidbody3D>();
#endif 
            tracker = GetComponent<PositionTracker>();
        }

        private void Start()
        {
            if (materialSettings == null)
            {
                materialSettings = PositionTrackerMaterialSettings.DefaultSettings();
            }
        }

        #region PositionTracker.IPositionTrackerExtension

        public DebugRoot.StateInfo StateInfoState(PositionTracker.LoggedState state)
        {
            if (velocityAverageInfoByState.ContainsKey(state) == false)
            {
                velocityAverageInfoByState[state] = VelocityAverageInfo.InitialState();
            }
            int maxPositiveStep = (int)(maxError / gapSteps);
            var velocityAverageInfo = velocityAverageInfoByState[state];
            DebugRoot.StateInfo info;
            string velocityStr = "";
            int pointMd = -1;
            float realVariation = 0f;
            var position = transform.position;
            var rotation = transform.rotation;
#if USE_PHYSICSADDON
            bool useInterpolationTarget = (state == PositionTracker.LoggedState.LateUpdateInterpolationTarget);
            useInterpolationTarget = useInterpolationTarget || (state == PositionTracker.LoggedState.FixedUpdateInterpolationTarget);
            useInterpolationTarget = useInterpolationTarget || (state == PositionTracker.LoggedState.RenderInterpolationTarget);
            if (useInterpolationTarget && nrb)
            {
                position = nrb.InterpolationTarget.position;
                rotation = nrb.InterpolationTarget.rotation;
            }
#endif
            if (tracker && tracker.debugRotation == false)
            {
                rotation = Quaternion.identity;
            }
            if (debugRenderVelocity)
            {
                var dt = 0f;
                var v = Vector3.zero;
                if (velocityAverageInfo.lastRenderTime != -1)
                {
                    dt = Time.time - velocityAverageInfo.lastRenderTime;
                    var move = position - velocityAverageInfo.lastPos;
                    v = move / dt;
                    if (v.magnitude == 0)
                    {
                        //Debug.LogError($"transform.position:{transform.position} lastPos{lastPos} {Time.time} {lastRenderTime}");
                    }
                }
                velocityAverageInfo.lastRenderTime = Time.time;
                velocityAverageInfo.lastPos = position;

                if (useGlobalAverageVelocity)
                {
                    // Global average
                    velocityAverageInfo.magnitudeTotal += v.magnitude;
                    velocityAverageInfo.magnitudeCount++;
                    velocityAverageInfo.magnitudeAverage = velocityAverageInfo.magnitudeTotal / velocityAverageInfo.magnitudeCount;
                }
                else
                {
                    // Local average
                    velocityAverageInfo.velBuffer[velocityAverageInfo.ringBufferCursor] = v.magnitude;
                    if (velocityAverageInfo.magnitudeCount < VelocityAverageInfo.BUFFER_SIZE)
                    {
                        velocityAverageInfo.magnitudeCount = velocityAverageInfo.ringBufferCursor + 1;
                    }
                    velocityAverageInfo.ringBufferCursor = (velocityAverageInfo.ringBufferCursor + 1) % VelocityAverageInfo.BUFFER_SIZE;
                    velocityAverageInfo.magnitudeTotal = 0;
                    for (int i = 0; i < velocityAverageInfo.magnitudeCount; i++) velocityAverageInfo.magnitudeTotal += velocityAverageInfo.velBuffer[i];
                    velocityAverageInfo.magnitudeAverage = velocityAverageInfo.magnitudeTotal / velocityAverageInfo.magnitudeCount;
                }


                float gap = 0;
                if (velocityAverageInfo.magnitudeAverage != 0)
                {
                    gap = (v.magnitude - velocityAverageInfo.magnitudeAverage) / velocityAverageInfo.magnitudeAverage;
                    gap = Mathf.Clamp(gap, -maxError, maxError);
                    realVariation = gap / (float)maxError;
                    bool underAcceptedGap = Mathf.Abs(gap) <= acceptedGap;
                    if (underAcceptedGap)
                    {
                        gap = 0;
                    }
                    if (underAcceptedGap == false || useRenderMaterialUnderAcceptedGap == false)
                    {
                        pointMd = (int)(gap / gapSteps);
                        pointMd += maxPositiveStep;
                    }
                }
                else
                {
                    // Recentered 0
                    pointMd = maxPositiveStep;
                }
                velocityStr = $" (dt:{dt}/v:{v.magnitude}/avgv:{velocityAverageInfo.magnitudeAverage} - {v} " +
                    $"// Gap:{gap} -> non centered md:{gap / gapSteps} -> pointMd:{pointMd}/{2f * maxError / gapSteps}) " +
                    $"// Var: {realVariation}";
            }
            string text = $"{Time.time}{velocityStr}";
            info.md = pointMd;
            info.name = text;
            info.rotation = rotation;
            velocityAverageInfoByState[state] = velocityAverageInfo;
            return info;
        }

        public Material MetadataMaterial(int md)
        {
            if (materialSettings == null)
            {
                Debug.LogError("Can not found the PositionTrackerMaterialSettings");
                return null;
            }

            velocityChangeMaterial = materialSettings.velocityChangeMaterial;
            if (velocityChangeMaterial == null)
            {
                Debug.LogError("Missing base material for velocity debug");
                return null;
            }
            var material = new Material(velocityChangeMaterial);

            float maxStep = 2f * maxError / gapSteps;
            var level = Mathf.Clamp01((float)md / maxStep);

            var centerColor = Color.black;
            centerColor.a = 0.29f;
            if (level > 0.5f)
            {
                material.color = Color.Lerp(centerColor, Color.red, (level - 0.5f) * 2);
            }
            else
            {
                material.color = Color.Lerp(centerColor, Color.blue, (0.5f - level) * 2);
            }
            return material;
        }
        #endregion

        public static Vector3 AngularVelocityChange(Quaternion previousRotation, Quaternion newRotation, float elapsedTime)
        {
            Quaternion rotationStep = newRotation * Quaternion.Inverse(previousRotation);
            rotationStep.ToAngleAxis(out float angle, out Vector3 axis);
            // Angular velocity uses eurler notation, bound to -180° / +180°
            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (Mathf.Abs(angle) > Mathf.Epsilon)
            {
                float radAngle = angle * Mathf.Deg2Rad;
                Vector3 angularStep = axis * radAngle;
                Vector3 angularVelocity = angularStep / elapsedTime;
                if (!float.IsNaN(angularVelocity.x))
                    return angularVelocity;
            }
            return Vector3.zero;
        }
    }
}
