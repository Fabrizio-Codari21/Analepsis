using System;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] protected CheckIntAmount m_checkInt;
    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    
    private List<Tip> tips = new();
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
        if (tips.Count == 0) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var t in tips) sb.Append(t.tip + " ");
        

        return sb.ToString();
    }

    public void AddTip(Tip tip)
    {
        int insertIndex = tips.Count;
        for (int i = 0; i < tips.Count; i++)
        {
            if (tip.order >= tips[i].order) continue;
            insertIndex = i;
            break;
        }

        tips.Insert(insertIndex, tip);
    }

    public void RemoveTip(Tip tip)
    {
        tips.Remove(tip);
    }
    
}

public interface IInspectable
{
    public Item GetInspectItem();
}

