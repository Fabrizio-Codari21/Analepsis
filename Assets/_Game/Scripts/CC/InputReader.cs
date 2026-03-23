using UnityEngine;

public abstract class InputReader : ScriptableObject
{

    public abstract void SetCallback(InputActions inputAction);

    public abstract void SetEnable(InputActions inputAction, bool enable = true);
    
}
