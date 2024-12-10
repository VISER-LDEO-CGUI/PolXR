using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fusion.XR.Host.Grabbing
{
    /**
     * This optional components display a ghost of a graqbbed object at the position it would be if it was not colliding, thus following the ghost hand
     */
    [RequireComponent(typeof(NetworkPhysicsGrabbable))]
    [DefaultExecutionOrder(PseudoHapticGrabbableRender.EXECUTION_ORDER)]
    public class PseudoHapticGrabbableRender : MonoBehaviour
    {
        public const int EXECUTION_ORDER = NetworkPhysicsGrabbable.EXECUTION_ORDER + 10;
        public Material ghostMetarial;

        public GameObject grabbableVisualPart;
        
        NetworkPhysicsGrabbable networkGrabbable;
        bool isPseudoHapticDisplayed = false;

        GameObject ghostObject;
        Renderer[] ghostRenderers;

        void Awake()
        {
            networkGrabbable = GetComponent<NetworkPhysicsGrabbable>();
            if (grabbableVisualPart == null) Debug.LogError("The visual part has to be specified to know what to copy as the ghost render part");
        }

        void CreateGhost()
        {
            ghostObject = GameObject.Instantiate(grabbableVisualPart);
            ghostObject.transform.localScale = grabbableVisualPart.transform.lossyScale;
            ghostObject.transform.parent = grabbableVisualPart.transform;
            ghostRenderers = ghostObject.GetComponentsInChildren<Renderer>();
            if(ghostMetarial == null)
            {
                if (networkGrabbable.CurrentGrabber && networkGrabbable.CurrentGrabber.hand.LocalHardwareHand && networkGrabbable.CurrentGrabber.hand.LocalHardwareHand.localHandRepresentation != null)
                {
                    ghostMetarial = networkGrabbable.CurrentGrabber.hand.LocalHardwareHand.localHandRepresentation.SharedHandMaterial;
                }
            }
            if (ghostMetarial)
            {
                var material = networkGrabbable.CurrentGrabber.hand.LocalHardwareHand.localHandRepresentation.SharedHandMaterial;
                foreach(var renderer in ghostRenderers)
                {
                    renderer.sharedMaterial = ghostMetarial = networkGrabbable.CurrentGrabber.hand.LocalHardwareHand.localHandRepresentation.SharedHandMaterial;
                }
            }
        }

        void SetGhostVisibility(bool visible)
        {
            foreach (var renderer in ghostRenderers)
            {
                renderer.enabled = visible;
            }
            ghostObject.SetActive(visible);
        }

        private void LateUpdate()
        {
            if (networkGrabbable.isPseudoHapticDisplayed && networkGrabbable.CurrentGrabber && networkGrabbable.CurrentGrabber.hand.LocalHardwareHand)
            {
                if (!isPseudoHapticDisplayed)
                {
                    // Display ghost object
                    if (!ghostObject) CreateGhost();
                    SetGhostVisibility(true);
                    isPseudoHapticDisplayed = true;
                }

                // Move ghost object: follow ghost hand
                Transform ghostHand = networkGrabbable.CurrentGrabber.hand.LocalHardwareHand.transform;
                ghostObject.transform.position = ghostHand.TransformPoint(networkGrabbable.grabbable.localPositionOffset);
                ghostObject.transform.rotation = ghostHand.rotation * networkGrabbable.grabbable.localRotationOffset;
            } else
            {
                if (isPseudoHapticDisplayed)
                {
                    // Hide ghost object
                    SetGhostVisibility(false);
                    isPseudoHapticDisplayed = false;
                }
            }
        }
    }

}
