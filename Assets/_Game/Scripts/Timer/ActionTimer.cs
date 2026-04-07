using System;
using Sirenix.OdinInspector;
using UnityEngine;
public class ActionTimer : MonoBehaviour
{
    [ShowInInspector,ReadOnly] private int _actionLeft;
    [SerializeField] private int m_maxActionsLevel;
    [SerializeField] private CheckIntAmount m_checkActionAmount;
    public event Action OnActionFinish;
    private void Start()
    {
        // m_checkActionAmount.OnRequest += TryCostAction;
    }
    private bool TryCostAction(int cost)
    {
        if(_actionLeft < cost) return false; // si no hay suficiente actione
        _actionLeft -= cost; 
        return true;
    }
    private void Finish()
    {
        OnActionFinish?.Invoke();
    }
    private void OnDestroy()
    {
       // m_checkActionAmount.OnRequest -= TryCostAction;
    }
}

public interface IAction
{
    public int Cost { get ; set; }
    public void RequiredConsume();
}