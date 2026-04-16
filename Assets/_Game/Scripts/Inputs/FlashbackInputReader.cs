using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Flashback Input", menuName = "Game/InputReader/Flashback")]
public class FlashbackInputReader : InputReader, InputActions.IFlashbackActions
{
    public CCInputReader baseInput;

    public event Action ExitFlashback = delegate { };
    public event Action Interact = delegate { };

    public void OnExit(InputAction.CallbackContext context)
    {
        if(context.started) ExitFlashback?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.started) Interact?.Invoke();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        baseInput.OnLook(context);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        baseInput.OnMove(context);
    }

    public override void SetEnable(bool enable = true)
    {
        if (enable) InputAction?.Flashback.Enable();
        else InputAction?.Flashback.Disable();
    }

    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.Flashback.SetCallbacks(this);
    }


}
