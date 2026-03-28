using System;
using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public GameObject InteractableObject => gameObject;
    public TextMeshProUGUI interactText;
    public int actionCost = 0;

    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    public virtual void InteractStart()
    {
        Debug.Log("InteractStart");
        OnStart?.Invoke();
    }

    public virtual void InteractEnd()
    {
        Debug.Log("InteractEnd");
        OnEnd?.Invoke();
    }
    

    public virtual void Focus()
    { 
        Debug.Log("Focus");
        OnFocus?.Invoke();
    }

    public virtual void Unfocus()
    {
        Debug.Log("Unfocus");
        OnUnfocus?.Invoke();
    }
}

public interface IInspectable
{
    public Item GetInspectItem();
}