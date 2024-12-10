using Fusion.XR.Host.Grabbing;
using UnityEngine;


namespace Fusion.XR.Host.Rig
{
    /**
     * 
     * Networked VR user
     * 
     * Handle the synchronisation of the various rig parts: headset, left hand, right hand, and playarea (represented here by the NetworkRig)
     * Use the local HardwareRig rig parts position info when this network rig is associated with the local user 
     * 
     * 
     **/

    [RequireComponent(typeof(NetworkTransform))]
    // We ensure to run after the NetworkTransform or NetworkRigidbody, to be able to override the interpolation target behavior in Render()
    [DefaultExecutionOrder(NetworkRig.EXECUTION_ORDER)]
    public class NetworkRig : NetworkBehaviour
    {
        public const int EXECUTION_ORDER = 100;
        public HardwareRig hardwareRig;
        public NetworkHand leftHand;
        public NetworkHand rightHand;
        public NetworkHeadset headset;
        public NetworkGrabber leftGrabber;
        public NetworkGrabber rightGrabber;

        [HideInInspector]
        public NetworkTransform networkTransform;

        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            leftGrabber = leftHand.GetComponent<NetworkGrabber>();
            rightGrabber = rightHand.GetComponent<NetworkGrabber>();
        }

        // As we are in host topology, we use the input authority to track which player is the local user
        public bool IsLocalNetworkRig => Object.HasInputAuthority;

        public override void Spawned()
        {
            base.Spawned();
            if (IsLocalNetworkRig)
            {
                hardwareRig = FindObjectOfType<HardwareRig>();
                if (hardwareRig == null) Debug.LogError("Missing HardwareRig in the scene");
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            // update the rig at each network tick
            if (GetInput<RigInput>(out var input))
            {
                transform.position = input.playAreaPosition;
                transform.rotation = input.playAreaRotation;
                leftHand.transform.position = input.leftHandPosition;
                leftHand.transform.rotation = input.leftHandRotation;
                rightHand.transform.position = input.rightHandPosition;
                rightHand.transform.rotation = input.rightHandRotation;
                headset.transform.position = input.headsetPosition;
                headset.transform.rotation = input.headsetRotation;
                // we update the hand pose info. It will trigger on network hands OnHandCommandChange on all clients, and update the hand representation accordingly
                leftHand.HandCommand = input.leftHandCommand;
                rightHand.HandCommand = input.rightHandCommand;

                leftGrabber.GrabInfo = input.leftGrabInfo;
                rightGrabber.GrabInfo = input.rightGrabInfo;
            }
        }

        public override void Render()
        {
            base.Render();
            if (IsLocalNetworkRig)
            {
                // Extrapolate for local user:
                // we want to have the visual at the good position as soon as possible, so we force the visuals to follow the most fresh hardware positions
                // To update the visual object, and not the actual networked position, we move the interpolation targets
                transform.position = hardwareRig.transform.position;
                transform.rotation = hardwareRig.transform.rotation;
                leftHand.transform.position = hardwareRig.leftHand.transform.position;
                leftHand.transform.rotation = hardwareRig.leftHand.transform.rotation;
                rightHand.transform.position = hardwareRig.rightHand.transform.position;
                rightHand.transform.rotation = hardwareRig.rightHand.transform.rotation;
                headset.transform.position = hardwareRig.headset.transform.position;
                headset.transform.rotation = hardwareRig.headset.transform.rotation;
            }
        }
    }
}
