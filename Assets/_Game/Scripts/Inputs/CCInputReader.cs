using System;
using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(fileName = "CCInputReader", menuName = "Game/InputReader/Player")]
public class CCInputReader : InputReader,InputActions.IPlayerActions
{
    
    public event Action<Vector2> Move = delegate { };

    
    public event Action InteractPressed = delegate { };
    public event Action InteractReleased = delegate { };

    public event Action OpenNotebook = delegate { };
    public event Action OpenTheoryBoard = delegate { };
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started) return;
        Move?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
       
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started) InteractPressed?.Invoke();

        if (context.canceled) InteractReleased?.Invoke();
    }

    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.Player.SetCallbacks(this);
    }

    public override void SetEnable(bool enable = true)
    {
       if(enable) InputAction.Player.Enable();
       else InputAction.Player.Disable();
    }

    public void OnOpenNotebook(InputAction.CallbackContext context)
    {
        if (context.started)
        {      
            OpenNotebook?.Invoke();
        }
    }

    public void OnOpenTheoryBoard(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OpenTheoryBoard?.Invoke();
        }
    }
}
