using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class InputHandler : InputSystemGlobalHandlerListener, IMixedRealityInputHandler
{
    [Tooltip("The Input Action to be mapped to grip down")]
    public MixedRealityInputAction GripDown;

    [Tooltip("Multiplier of vertical movement, below 1 is a good value")]
    public float multiplier = 0.05f;

    public void OnInputDown(InputEventData eventData)
    {
        if (eventData.MixedRealityInputAction.Equals(GripDown))
        {
            int i = 0;
            foreach (IMixedRealityController controller in CoreServices.InputSystem.DetectedControllers)
            {
                if (eventData.InputSource.Equals(controller.InputSource) && i == 1)
                {
                    MixedRealityPlayspace.Transform.Translate(0, multiplier, 0);
                } else if(eventData.InputSource.Equals(controller.InputSource) && i == 0)
                {
                    MixedRealityPlayspace.Transform.Translate(0, -1 * multiplier, 0);
                }
                i = 1;
            }
        }
    }

    public void OnInputUp(InputEventData eventData)
    {
        return;
    }

    public void Update()
    {
    }

    protected override void RegisterHandlers()
    {
        CoreServices.InputSystem.RegisterHandler<IMixedRealityInputHandler>(this);
    }

    protected override void UnregisterHandlers()
    {
        CoreServices.InputSystem.UnregisterHandler<IMixedRealityInputHandler>(this);
    }
}
