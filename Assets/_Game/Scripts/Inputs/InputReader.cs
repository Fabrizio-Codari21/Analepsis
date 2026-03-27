using UnityEngine;

public abstract class InputReader : ScriptableObject
{
    
    [SerializeField] public bool isAutoEnable = true;
    [HideInInspector] public InputActions InputAction;


    public virtual void Initialize(InputActions inputActions)
    {
        InputAction = inputActions;
        SetCallback(inputActions);
    }
    protected abstract void SetCallback(InputActions inputAction);
    public abstract void SetEnable(bool enable = true);
    
}