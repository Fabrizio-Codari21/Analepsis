using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionConsumer : MonoBehaviour,IAction
{
    [SerializeField] private CheckIntAmount m_consumeAction;
    [SerializeField] private CheckIntAmount m_haveAction;
    [SerializeField] private int cost;
    private IInteractable _interactable;
    private ITipProvider  _tipProvider;
    private IConditionCheck _conditionCheck;
    private void Awake()
    {
        _interactable = GetComponent<IInteractable>();
        _tipProvider = GetComponent<ITipProvider>();
        _conditionCheck = GetComponent<IConditionCheck>();
        _interactable.OnStart += RequiredConsume;
        _conditionCheck?.Conditions.Add(new NotAction((() => m_haveAction.Request(cost))));
        // _conditionCheck?.Conditions.Add(new NotAction((
        //     () => FlashbackManager.Instance.GetFlashbackObject() != _interactable 
        //     ? !FlashbackManager.Instance.IsFlashbackOn()
        //     : true), "Can't interact during a flashback; press 'F' to leave."));
    }

    private void Start()
    {
        _tipProvider.AddTip(new Tip(
            $"[Costs {Cost} action{((Cost > 1 || Cost <= 0) ? "s" : null)}.]",
            TipOrder.ActionCost));
    }

    public int Cost
    {
        get => cost;
        set => cost = value;
    }
    public void RequiredConsume()
    {
        m_consumeAction.Request(Cost);
    }
}

public interface IConditionCheck
{
    public List<ICondition>  Conditions { get; }
}

public interface ICondition
{
    public bool Check();
    string GetFailureTip();
}

public class NotAction : ICondition
{
    
    Func<bool> m_checkAction;
    string _failureTip;

    public NotAction(Func<bool> checkAction, string failureTip = "I've run out of actions: time to solve the case...")
    {
        m_checkAction = checkAction;
        _failureTip = failureTip;
    }
    
    public bool Check()
    {
        return m_checkAction();
    }

    public string GetFailureTip()
    {
        return _failureTip;
    }
}