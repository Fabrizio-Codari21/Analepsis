using UnityEngine;

public abstract class InputReader : ScriptableObject
{

    public InputActions InputActions;

    public virtual void SetCallback(InputActions inputAction)
    {
        InputActions = inputAction;
    }

    public abstract void SetEnable(InputActions inputAction, bool enable = true);
    
}
