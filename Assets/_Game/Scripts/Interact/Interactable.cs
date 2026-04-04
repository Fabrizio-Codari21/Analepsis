using System;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    public virtual void InteractStart()
    {
        OnStart?.Invoke();
    }
    public virtual void InteractEnd()
    {
      
        OnEnd?.Invoke();
    }
    public virtual void Focus()
    { 
        OnFocus?.Invoke();
    }

    public virtual void Unfocus()
    {
        OnUnfocus?.Invoke();
    }

    public string GetTip()
    {
        return "Interact";
    }
}

public interface IInspectable
{
    public Item GetInspectItem();
}

public interface IActionConsumable
{
    public int ActionConsumed { get; set; }
}
