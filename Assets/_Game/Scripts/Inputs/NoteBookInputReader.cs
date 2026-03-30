using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "NoteBookInputReader", menuName = "Game/InputReader/NoteBook")]
public class NoteBookInputReader : InputReader, InputActions.INoteBookActions
{


    public event Action Close = delegate { };
    public void OnClose(InputAction.CallbackContext context)
    {
       if(context.started) Close?.Invoke(); 
    }

    public override void SetEnable(bool enable = true)
    {
        if (enable) InputAction?.NoteBook.Enable();
        else InputAction?.NoteBook.Disable();
    }

    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.NoteBook.SetCallbacks(this);
    }
}