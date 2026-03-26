using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    public GameObject InteractableObject => gameObject;
    
    public virtual void InteractStart()
    {
        Debug.Log("InteractStart");
    }

    public virtual void InteractEnd()
    {
        Debug.Log("InteractEnd");
    }
    

    public virtual void Focus()
    { 
        Debug.Log("Focus");
    }

    public virtual void Unfocus()
    {
        Debug.Log("Unfocus");
    }
    
    
}