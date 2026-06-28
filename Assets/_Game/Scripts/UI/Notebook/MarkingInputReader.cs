using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "MarkInputReader", menuName = "Game/InputReader/MarkPanel")]
public class MarkingInputReader : InputReader ,InputActions.IMarkPanelActions
{
    
    public event Action Confirm = delegate { };
    public event Action Cancel = delegate { };
    protected override void RemoveCallback(InputActions inputAction)
    {
        inputAction?.MarkPanel.SetCallbacks(this);
    }

    public override void SetEnable(bool enable = true)
    {
        if (enable) InputAction?.MarkPanel.Enable();
        else InputAction?.MarkPanel.Disable();
    }

    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.MarkPanel.SetCallbacks(this);
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if(context.started) Confirm?.Invoke();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if(context.started) Cancel?.Invoke();
    }
}