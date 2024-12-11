//using System.Collections;
//using UnityEngine;
//using Microsoft.MixedReality.Toolkit;
//using Microsoft.MixedReality.Toolkit.Input;
//using Microsoft.MixedReality.Toolkit.Utilities;

//public class InputHandler : InputSystemGlobalHandlerListener, IMixedRealityInputHandler
//{
//    [Tooltip("The Input Action to be mapped to grip down")]
//    public MixedRealityInputAction GripDown;

//    [Tooltip("Multiplier of vertical movement, below 1 is a good value")]
//    public float multiplier = 0.05f;
//    public float delay = 1f;

//    private bool rightHandGripPressed = false;
//    private bool leftHandGripPressed = false;
//    private Coroutine rightHandCoroutine;
//    private Coroutine leftHandCoroutine;

//    public void OnInputDown(InputEventData eventData)
//    {
//        if (eventData.MixedRealityInputAction.Equals(GripDown))
//        {
//            int i = 0;
//            foreach (IMixedRealityController controller in CoreServices.InputSystem.DetectedControllers)
//            {
//                if (eventData.InputSource.Equals(controller.InputSource))
//                {
//                    if (i == 0)
//                    {
//                        leftHandGripPressed = true;
//                        if (leftHandCoroutine == null)
//                            leftHandCoroutine = StartCoroutine(AdjustHeight(false));
//                    }
//                    else if (i == 1)
//                    {
//                        rightHandGripPressed = true;
//                        if (rightHandCoroutine == null)
//                            rightHandCoroutine = StartCoroutine(AdjustHeight(true));
//                    }
//                }
//                i++;
//            }
//        }
//    }

//    public void OnInputUp(InputEventData eventData)
//    {
//        if (eventData.MixedRealityInputAction.Equals(GripDown))
//        {
//            int i = 0;
//            foreach (IMixedRealityController controller in CoreServices.InputSystem.DetectedControllers)
//            {
//                if (eventData.InputSource.Equals(controller.InputSource))
//                {
//                    if (i == 0)
//                    {
//                        leftHandGripPressed = false;
//                        if (leftHandCoroutine != null)
//                        {
//                            StopCoroutine(leftHandCoroutine);
//                            leftHandCoroutine = null;
//                        }
//                    }
//                    else if (i == 1)
//                    {
//                        rightHandGripPressed = false;
//                        if (rightHandCoroutine != null)
//                        {
//                            StopCoroutine(rightHandCoroutine);
//                            rightHandCoroutine = null;
//                        }
//                    }
//                }
//                i++;
//            }
//        }
//    }

//    private IEnumerator AdjustHeight(bool isRightHand)
//    {
//        yield return new WaitForSeconds(delay); // Wait for 1 second before starting smooth movement

//        while ((isRightHand && rightHandGripPressed) || (!isRightHand && leftHandGripPressed))
//        {
//            float verticalMovement = multiplier * Time.deltaTime * (isRightHand ? 1 : -1);
//            MixedRealityPlayspace.Transform.Translate(0, verticalMovement, 0);
//            yield return null;
//        }
//    }

//    protected override void RegisterHandlers()
//    {
//        CoreServices.InputSystem.RegisterHandler<IMixedRealityInputHandler>(this);
//    }

//    protected override void UnregisterHandlers()
//    {
//        CoreServices.InputSystem.UnregisterHandler<IMixedRealityInputHandler>(this);
//    }
//}
