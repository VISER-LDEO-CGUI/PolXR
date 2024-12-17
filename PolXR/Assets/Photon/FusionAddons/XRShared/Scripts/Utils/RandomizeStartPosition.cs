using Fusion.XR.Shared.Desktop;
using Fusion.XR.Shared.Rig;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.XR;

/**
 * 
 * RandomizeStartPosition is in charge to spawn users at a random position 
 * 
 **/

namespace Fusion.XR.Shared
{
    public class RandomizeStartPosition : MonoBehaviour
    {
        public Transform startCenterPosition;
        public float randomRadius = 10;
        public bool useNavMesh = true;
        public bool lookAtCenterPosition = false;

        [Header("VR")]
        public bool shouldAlignHeadsetInsteadOfRigInVR = true;

        public Transform childHeadsetToAlign = null;
#if ENABLE_INPUT_SYSTEM
        public InputActionProperty headsetAvailableAction = new InputActionProperty();
#endif

        public enum DetectionMode
        {
            TrackingState,  // Relevant for OpenXR Plugin
            UserPresence    // Relevant for Oculus Plugin
        }

        [Tooltip("Use User presence when using the Oculus XR plugin, TrackingState when using the OpenXR plugin")]

        public DetectionMode detectionMode = DetectionMode.TrackingState;

        bool positionSet = false;
        private void Awake()
        {
            if (GetComponent<DesktopController>())
            {
                // Not in VR
                shouldAlignHeadsetInsteadOfRigInVR = false;
            }
            else
            {
                // VR
                var headset = GetComponentInChildren<HardwareHeadset>();
                if (headset && childHeadsetToAlign == null) childHeadsetToAlign = headset.transform;
            }

            FindStartPosition();
        }

        public void FindStartPosition()
        {

            int tries = 0;
            if (startCenterPosition == null) startCenterPosition = transform;

            // try to 10 times to find a valid destination point near the random position
            bool positionFound = false;

            if (useNavMesh == false)
            {
                Vector3 pos = startCenterPosition.position + randomRadius * Random.insideUnitSphere;
                pos = new Vector3(pos.x, startCenterPosition.position.y, pos.z);
                transform.position = pos;
                transform.rotation = startCenterPosition.rotation;
                Debug.Log("Placed rig at start position " + transform.position + ", around " + startCenterPosition.position);
                positionFound = true;
            }
            else
            {   // use nav mesh to find a destination
                while (tries < 10)
                {
                    Vector3 pos = startCenterPosition.position + randomRadius * Random.insideUnitSphere;
                    pos = new Vector3(pos.x, startCenterPosition.position.y, pos.z);
                    // check if a destination has been found near the random position

                    if (NavMesh.SamplePosition(pos, out var hit, 1f, NavMesh.AllAreas))
                    {
                        transform.position = hit.position;
                        transform.rotation = startCenterPosition.rotation;
                        Debug.Log("Placed rig at start position " + transform.position + ", around " + startCenterPosition.position);
                        positionFound = true;
                        break;
                    }
                    tries++;
                }
                if (!positionFound)
                {
                    Debug.LogError("Unable to find random start position around " + startCenterPosition.position + ". Is NavMesh set ?");
                    transform.position = startCenterPosition.position;
                    transform.rotation = startCenterPosition.rotation;
                }
            }
            positionSet = true;

#if ENABLE_INPUT_SYSTEM
            switch (detectionMode)
            {
                case DetectionMode.TrackingState:
                    headsetAvailableAction.EnableWithDefaultXRBindings(new List<string> { "<XRHMD>/trackingState" });
                    break;

                case DetectionMode.UserPresence:
                    headsetAvailableAction.EnableWithDefaultXRBindings(new List<string> { "<XRHMD>/userPresence" });
                    break;
            }
#endif

        }

        void Start()
        {
            AlignHeadsetInVR();
        }


        public bool HeadsetDetected
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                switch (detectionMode)
                {
                    case DetectionMode.TrackingState:
                        InputTrackingState trackingState = (InputTrackingState)headsetAvailableAction.action.ReadValue<int>();
                        return trackingState != InputTrackingState.None;

                    case DetectionMode.UserPresence:
                        float userPresence = headsetAvailableAction.action.ReadValue<float>();
                        return userPresence != 0;
                }
#endif
                return false;
            }
        }


        async void AlignHeadsetInVR()
        {
            if (!shouldAlignHeadsetInsteadOfRigInVR) return;
            if (!childHeadsetToAlign) return;

            while (!positionSet) await AsyncTask.Delay(10);

            float waitEnd = Time.time + 10;
            while (HeadsetDetected == false && Time.time < waitEnd) await AsyncTask.Delay(100);
            if (HeadsetDetected == false)
            {
                Debug.LogError("Wait timeout for VR headset detection");
                return;
            }

            if (lookAtCenterPosition)
            {
                var childHeadsetProjection = childHeadsetToAlign.transform.position;
                childHeadsetProjection.y = startCenterPosition.position.y;
                var targetRotation = Quaternion.LookRotation(startCenterPosition.position - childHeadsetProjection);

                var localChildRotation = Quaternion.Inverse(transform.rotation) * childHeadsetToAlign.rotation;
                var alignRotation = Quaternion.Inverse(localChildRotation) * targetRotation;
                transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, alignRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                Debug.Log("Aligned headset to look at start position");

            }
            else
            {
                var localChildRotation = Quaternion.Inverse(transform.rotation) * childHeadsetToAlign.rotation;
                var alignRotation = Quaternion.Inverse(localChildRotation) * startCenterPosition.rotation;
                transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, alignRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                Debug.Log("Aligned headset with start position");
            }
        }
    }
}
