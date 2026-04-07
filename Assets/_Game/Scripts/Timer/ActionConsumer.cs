using System;
using UnityEngine;

public class ActionConsumer : MonoBehaviour,IAction
{
    [SerializeField] private CheckIntAmount m_checkActionAmount;
    [SerializeField] private int cost;
    private IInteractable _interactable;
    private ITipProvider  _tipProvider;
    private void Awake()
    {
        _interactable = GetComponent<IInteractable>();
        _tipProvider = GetComponent<ITipProvider>();
        
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
        m_checkActionAmount.Request(Cost);
    }
}