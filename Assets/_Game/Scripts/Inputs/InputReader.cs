using UnityEngine;

public abstract class InputReader : ScriptableObject
{
    
    [SerializeField] public bool isAutoEnable = true;
    [HideInInspector] public InputActions InputAction;
    
    public virtual void Initialize(InputActions inputActions)
    {
        if (!Equals(InputAction, inputActions) && InputAction != null)
        {  
            RemoveCallback(InputAction);
        }
        InputAction = inputActions;
        SetCallback(inputActions);
    }


    protected abstract void SetCallback(InputActions inputAction);
    protected abstract void RemoveCallback(InputActions inputAction);
    public abstract void SetEnable(bool enable = true);
    
}