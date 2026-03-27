using UnityEngine;

public abstract class InputReader : ScriptableObject
{
    
    [SerializeField] public bool isAutoEnable = true;
    [HideInInspector] public InputActions InputAction;
    public virtual void SetCallback(InputActions inputAction)
    {
        InputAction = inputAction;
    }
    public abstract void SetEnable(bool enable = true);
    
}