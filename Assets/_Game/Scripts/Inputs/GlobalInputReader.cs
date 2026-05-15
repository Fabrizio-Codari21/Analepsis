using System;
using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(fileName = "Global Input",menuName = "Game/InputReader/Global")]
public class GlobalInputReader : InputReader, InputActions.IGlobalActions
{
    public event Action EscapePressed = delegate { };
    protected override void RemoveCallback(InputActions inputAction)
    {
        inputAction?.Global.RemoveCallbacks(this);
    }

    public override void SetEnable( bool enable = true)
    { 
        if(enable) InputAction?.Global.Enable();
        else InputAction?.Global.Disable();
    }
    public void OnEscape(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Debug.Log("Escape pressed");
        EscapePressed?.Invoke();
    }
    
    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.Global.SetCallbacks(this);
    }

}