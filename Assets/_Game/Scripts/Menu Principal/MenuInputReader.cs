using System;
using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(fileName = "Menu", menuName = "Game/InputReader/Menu Principal")]
public class MenuInputReader : InputReader,InputActions.IMenuActions
{
    public Action Touch =  delegate { };

    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.Menu.SetCallbacks(this);
    }

    public override void SetEnable(bool enable = true)
    {
   
        if(enable) InputAction?.Menu.Enable();
        else InputAction?.Menu.Disable();
    }

    public void OnTouch(InputAction.CallbackContext context)
    {
        if (context.started)  Touch ?.Invoke();
    }
    
}
