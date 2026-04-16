using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Flashback Input", menuName = "Game/InputReader/Flashback")]
public class FlashbackInputReader : CCInputReader, InputActions.IFlashbackActions
{
    public event Action ExitFlashback = delegate { };
    public void OnExit(InputAction.CallbackContext context)
    {
        if(context.started) ExitFlashback?.Invoke();
    }

}
