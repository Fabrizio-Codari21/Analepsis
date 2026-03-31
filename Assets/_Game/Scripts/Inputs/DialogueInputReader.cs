using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Dialogue Input",menuName = "Game/InputReader/Dialogue")]
public class DialogueInputReader : InputReader,InputActions.IDialogueActions
{
     
    public event Action Skip =  delegate { };
     
    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.Dialogue.SetCallbacks(this);
    }
    public override void SetEnable(bool enable = true)
    {
        if(enable) InputAction?.Dialogue.Enable();
        else InputAction?.Dialogue.Disable();
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        if(context.started) Skip?.Invoke();
    }
}