using System;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable,IAction
{
    [SerializeField] protected Item m_item;
    [SerializeField] protected CheckIntAmount m_checkInt;
    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    public virtual void InteractStart()
    {
        // if(!m_checkInt.Request(Cost)) return; // este evente debe esta bindeado un check de amount de actiones
        OnStart?.Invoke();
    }
    public virtual void InteractEnd()
    {
        // if(!m_checkInt.Request(Cost)) return;
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

    [SerializeField] private int cost;
    public int Cost { get => cost ; set => cost = value; }
    
    public void RequiredConsume()
    {
        throw new NotImplementedException();
    }
}

public interface IInspectable
{
    public Item GetInspectItem();
}

