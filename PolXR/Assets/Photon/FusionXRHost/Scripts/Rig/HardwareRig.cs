using Fusion.Sockets;
using Fusion.XR.Host.Grabbing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Fusion.XR.Host.Rig
{
    public enum RigPart
    {
        None,
        Headset,
        LeftController,
        RightController,
        Undefined
    }

    // Include all rig parameters in an network input structure
    public struct RigInput : INetworkInput
    {
        public Vector3 playAreaPosition;
        public Quaternion playAreaRotation;
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;
        public Vector3 headsetPosition;
        public Quaternion headsetRotation;
        public HandCommand leftHandCommand;
        public HandCommand rightHandCommand;
        public GrabInfo leftGrabInfo;
        public GrabInfo rightGrabInfo;
    }

    /**
     * 
     * Hardware rig gives access to the various rig parts: head, left hand, right hand, and the play area, represented by the hardware rig itself
     *  
     * Can be moved, either instantanesously, or with a camera fade
     * 
     **/

    public class HardwareRig : MonoBehaviour, INetworkRunnerCallbacks
    {
        public HardwareHand leftHand;
        public HardwareHand rightHand;
        public HardwareHeadset headset;
        public NetworkRunner runner;

        public enum RunnerExpectations
        {
            NoRunner, // For offline usages
            PresetRunner,
            DetectRunner // should not be used in multipeer scenario
        }
        public RunnerExpectations runnerExpectations = RunnerExpectations.DetectRunner;

        bool searchingForRunner = false;

        [Header("Input interpolation")]
        public bool useInputInterpolation = false;
        public float interpolationDelay = 0.008f;
        XRHeadsetInputDevice headsetInputDevice;
        XRControllerInputDevice leftHandInputDevice;
        XRControllerInputDevice rightHandInputDevice;


        public float InterpolationDelay => interpolationDelay;

        public async Task<NetworkRunner> FindRunner()
        {
            while (searchingForRunner) await Task.Delay(10);
            searchingForRunner = true;
            if (runner == null && runnerExpectations != RunnerExpectations.NoRunner)
            {
                if (runnerExpectations == RunnerExpectations.PresetRunner || NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
                {
                    Debug.LogWarning("Runner has to be set in the inspector to forward the input");
                }
                else
                {
                    // Try to detect the runner
                    runner = FindObjectOfType<NetworkRunner>(true);
                    var searchStart = Time.time;
                    while (searchingForRunner && runner == null)
                    {
                        if (NetworkRunner.Instances.Count > 0)
                        {
                            runner = NetworkRunner.Instances[0];
                        }
                        if (runner == null)
                        {
                            await System.Threading.Tasks.Task.Delay(10);
                        }
                    }
                }
            }
            searchingForRunner = false;
            return runner;
        }

        protected virtual void Awake()
        {
            if (leftHand) leftHandInputDevice = leftHand.GetComponentInChildren<XRControllerInputDevice>();
            if (rightHand) rightHandInputDevice = rightHand.GetComponentInChildren<XRControllerInputDevice>();
            if (headset) headsetInputDevice = headset.GetComponentInChildren<XRHeadsetInputDevice>();

            if (leftHandInputDevice == null || rightHandInputDevice == null || headsetInputDevice == null)
            {
                useInputInterpolation = false;
            }
        }

        protected virtual async void Start()
        {
            await FindRunner();
            if (runner)
            {
                runner.AddCallbacks(this);
            }
        }

        private void OnDestroy()
        {
            if (searchingForRunner) Debug.LogError("Cancel searching for runner in HardwareRig");
            searchingForRunner = false;
            if (runner) runner.RemoveCallbacks(this);
        }


        #region Locomotion
        // Update the hardware rig rotation. This will trigger a Riginput network update
        public virtual void Rotate(float angle)
        {
            transform.RotateAround(headset.transform.position, transform.up, angle);
        }

        // Update the hardware rig position. This will trigger a Riginput network update
        public virtual void Teleport(Vector3 position)
        {
            Vector3 headsetOffet = headset.transform.position - transform.position;
            headsetOffet.y = 0;
            transform.position = position - headsetOffet;
        }

        // Teleport the rig with a fader
        public virtual IEnumerator FadedTeleport(Vector3 position)
        {
            if (headset.fader) yield return headset.fader.FadeIn();
            Teleport(position);
            if (headset.fader) yield return headset.fader.WaitBlinkDuration();
            if (headset.fader) yield return headset.fader.FadeOut();
        }

        // Rotate the rig with a fader
        public virtual IEnumerator FadedRotate(float angle)
        {
            if (headset.fader) yield return headset.fader.FadeIn();
            Rotate(angle);
            if (headset.fader) yield return headset.fader.WaitBlinkDuration();
            if (headset.fader) yield return headset.fader.FadeOut();
        }
        #endregion

        #region INetworkRunnerCallbacks

        // Prepare the input, that will be read by NetworkRig in the FixedUpdateNetwork
        public void OnInput(NetworkRunner runner, NetworkInput input) {
            RigInput rigInput = new RigInput();
            rigInput.playAreaPosition = transform.position;
            rigInput.playAreaRotation = transform.rotation;
            if (useInputInterpolation)
            {
                var leftHandInterpolationPose = leftHandInputDevice.InterpolatedPose(InterpolationDelay);
                var rightHandInterpolationPose = rightHandInputDevice.InterpolatedPose(InterpolationDelay);
                var headsetInterpolationPose = headsetInputDevice.InterpolatedPose(InterpolationDelay);
                rigInput.leftHandPosition = leftHandInterpolationPose.position;
                rigInput.leftHandRotation = leftHandInterpolationPose.rotation;
                rigInput.rightHandPosition = rightHandInterpolationPose.position;
                rigInput.rightHandRotation = rightHandInterpolationPose.rotation;
                rigInput.headsetPosition = headsetInterpolationPose.position;
                rigInput.headsetRotation = headsetInterpolationPose.rotation;
            } else
            {
                rigInput.leftHandPosition = leftHand.transform.position;
                rigInput.leftHandRotation = leftHand.transform.rotation;
                rigInput.rightHandPosition = rightHand.transform.position;
                rigInput.rightHandRotation = rightHand.transform.rotation;
                rigInput.headsetPosition = headset.transform.position;
                rigInput.headsetRotation = headset.transform.rotation;
            }

            rigInput.leftHandCommand = leftHand.handCommand;
            rigInput.rightHandCommand = rightHand.handCommand;

            rigInput.leftGrabInfo = leftHand.grabber.GrabInfo;
            rigInput.rightGrabInfo = rightHand.grabber.GrabInfo;

            input.Set(rigInput);
        }

        #endregion

        #region INetworkRunnerCallbacks (unused)
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        #endregion
    }
}
