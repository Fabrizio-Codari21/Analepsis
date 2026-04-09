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
    }

    private void Start()
    {
        _tipProvider.AddTip(new Tip("Action Cost :" + $"{Cost}",TipOrder.ActionCost));
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

    public NotAction(Func<bool> checkAction)
    {
        m_checkAction = checkAction;
    }
    
    public bool Check()
    {
        return m_checkAction();
    }

    public string GetFailureTip()
    {
        return "Insufficient Action";
    }
}