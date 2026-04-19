using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Flashback Input", menuName = "Game/InputReader/Flashback")]
public class FlashbackInputReader : InputReader, InputActions.IFlashbackActions
{
    
    public event Action ExitFlashback = delegate { };

    public void OnExit(InputAction.CallbackContext context)
    {
        if(context.started) ExitFlashback?.Invoke();
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
