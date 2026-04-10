using System;
using UnityEngine;
using UnityEngine.InputSystem;
[CreateAssetMenu(fileName = "Inspect Input",menuName = "Game/InputReader/Inspection")]
public class InspectionInputReader : InputReader, InputActions.IInspectionActions
{
    
    public event Action<Vector2> Rotate =  delegate { };
    public event Action<Vector2> PlaneRotate = delegate { };
    
    public event Action<bool> DragPressed = delegate { };
    
    public event Action<Vector2>  Scroll =  delegate { };
    
    public event Action Exit = delegate { };
    public event Action<Vector2> PointerMoved = delegate { };
    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.Inspection.SetCallbacks(this);
    }
    public override void SetEnable(bool enable = true)
    {
        if(enable) InputAction?.Inspection.Enable();
        else InputAction?.Inspection.Disable();
    }
    public void OnDelta(InputAction.CallbackContext context)
    {
        if (!InputAction.Inspection.Drag.IsPressed()) return;
        if (!context.performed) return;
        Rotate?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnDrag(InputAction.CallbackContext context)
    {
     
       if(context.started)DragPressed?.Invoke(true);
       if (context.canceled)DragPressed?.Invoke(false);
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        if(context.performed) Scroll?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnExit(InputAction.CallbackContext context)
    {
        if(context.canceled) Exit?.Invoke();
    }

    public void OnPlaneRotationDelta(InputAction.CallbackContext context)
    {
        if (!InputAction.Inspection.PlaneRotation.IsPressed()) return;
        if (!context.performed) return;

        Vector2 delta = context.ReadValue<Vector2>();
        PlaneRotate?.Invoke(delta);
    }

    private void OnMouseMove(InputAction.CallbackContext context)
    {
        
    }
    public void OnPlaneRotation(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            DragPressed?.Invoke(true);
        }

        if (context.canceled)
        {
            DragPressed?.Invoke(false);
        }
    }

    public void OnMovePointer(InputAction.CallbackContext context)
    {
        if (InputAction.Inspection.PlaneRotation.IsPressed()) return; 
        if (context.performed)
        {
            PointerMoved?.Invoke(context.ReadValue<Vector2>());
        }
    }
}