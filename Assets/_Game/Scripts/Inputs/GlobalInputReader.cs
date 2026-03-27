using System;
using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(fileName = "Global Input",menuName = "Game/InputReader/Global")]
public class GlobalInputReader : InputReader, InputActions.IGlobalActions
{
    public event Action EscapePressed = delegate { };
    public override void SetEnable( bool enable = true)
    { ;
        if(enable) InputAction?.Global.Enable();
        else InputAction?.Global.Disable();
    }
    public void OnEscape(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Debug.Log("Escape pressed");
        EscapePressed?.Invoke();
    }
    
    public override void SetCallback(InputActions inputAction)
    {
        base.SetCallback(inputAction);
        inputAction?.Global.SetCallbacks(this);
    }

}


public class InspectionInputReader : InputReader, InputActions.IInspectionActions
{
    public override void SetEnable(bool enable = true)
    {
        throw new NotImplementedException();
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
}