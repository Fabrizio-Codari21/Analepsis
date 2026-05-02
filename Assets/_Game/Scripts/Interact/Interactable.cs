using System;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable , IConditionCheck
{
    public List<ICondition> Conditions { get; } = new();
    bool ITipProvider.Override { get => _override; set => _override = value; }
    bool _override = false;

    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    
    private List<Tip> tips = new();

    //FlashbackManager.Instance.AddInteractable(gameObject);
    //public void Update()
    //{
    //    FlashbackManager.Instance.ToggleByFlashback(gameObject);
    //}



    public virtual void InteractStart()
    {
        var state = GetCurrentState();
        if(!state.canInteract || _override) return;
        OnStart?.Invoke();
    }
    public virtual void InteractEnd()
    {
        var state = GetCurrentState();
        if(!state.canInteract || _override) return;
        OnEnd?.Invoke();
    }
    public virtual void Focus()
    {
        if (_override) return;
        OnFocus?.Invoke();
    }

    public virtual void Unfocus()
    {
        if (_override) return;
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

    public void ClearTip()
    {
        tips.Clear();
    }
    public InteractionState GetCurrentState() // este para hacer un override de tip si no se puede interactuar
    {
        foreach (var condition in Conditions)
        {
            if (!condition.Check())
                return new InteractionState
                {
                    canInteract = false,
                    tipOverride = condition.GetFailureTip(),
                    tipColor = Color.red
                };
        }
        return new InteractionState
        {
            canInteract = true,
            tipOverride = GetTip(),
            tipColor = Color.white
        };
    }
}



public struct InteractionState
{
    public bool canInteract;  
    public string tipOverride; 
    public Color tipColor;      
}



public interface IInspectable
{
    public ItemReference GetItemReference();
}

