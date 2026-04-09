using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "TheoryBoardInputReader", menuName = "Game/InputReader/TheoryBoard")]
public class BoardInputReader : InputReader, InputActions.ITheoryBoardActions
{
    public event Action Close = delegate { };
    //public event Action<float> Flip = delegate { };
    public void OnClose(InputAction.CallbackContext context)
    {
        if (context.started) Close?.Invoke();
    }

    //public void OnFlip(InputAction.CallbackContext context)
    //{
    //    if (!context.started) return;
    //    Debug.Log(context.ReadValue<Vector2>());
    //    var value = context.ReadValue<Vector2>().x;
    //    Flip?.Invoke(value);
    //}

    public override void SetEnable(bool enable = true)
    {
        if (enable) InputAction?.TheoryBoard.Enable();
        else InputAction?.TheoryBoard.Disable();
    }

    protected override void SetCallback(InputActions inputAction)
    {
        inputAction?.TheoryBoard.SetCallbacks(this);
    }
}
