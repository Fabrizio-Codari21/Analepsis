using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InputReader : ScriptableObject
{
    
    
    public InputActions InputAction;

    public virtual void SetCallback(InputActions inputAction)
    {
        InputAction = inputAction;
    }

    public abstract void SetEnable(InputActions inputAction, bool enable = true);
    
}
