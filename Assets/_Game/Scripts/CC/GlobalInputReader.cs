using System;
using UnityEngine.InputSystem;

public class GlobalInputReader : InputReader, InputActions.IGlobalActions
{
    
    public event Action EscapePressed = delegate { };
    public override void SetEnable( bool enable = true)
    {
        if(enable) InputAction?.Global.Enable();
        else InputAction?.Global.Disable();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if(context.started) EscapePressed?.Invoke();
    }
}