using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

/// <summary>
/// Moves the player around the world using the gamepad, or any other input action that supports 2D axis.
/// 
/// We extend InputSystemGlobalHandlerListener because we always want to listen for the gamepad joystick position
/// We implement InputHandler<Vector2> interface in order to receive the 2D navigation action events.
/// </summary>
public class JoystickController : InputSystemGlobalHandlerListener, IMixedRealityInputHandler<Vector2>
{
    public MixedRealityInputAction navigationAction;
    public float multiplier = 5f;

    private Vector3 delta = Vector3.zero;
    public void OnInputChanged(InputEventData<Vector2> eventData)
    {
        float horiz = eventData.InputData.x;
        float vert = eventData.InputData.y;
        if (eventData.MixedRealityInputAction.Id == navigationAction.Id)
        {
            delta = CameraCache.Main.transform.TransformDirection(new Vector3(horiz, 0, vert) * multiplier);
        }
    }

    public void Update()
    {
        if (delta.sqrMagnitude > 0.01f)
        {
            MixedRealityPlayspace.Transform.Translate(delta);
        }
    }

    protected override void RegisterHandlers()
    {
        CoreServices.InputSystem.RegisterHandler<JoystickController>(this);
    }

    protected override void UnregisterHandlers()
    {
        CoreServices.InputSystem.UnregisterHandler<JoystickController>(this);
    }
}