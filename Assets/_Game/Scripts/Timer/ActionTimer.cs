using System;
using Sirenix.OdinInspector;
using UnityEngine;
public class ActionTimer : MonoBehaviour
{
    [ShowInInspector,ReadOnly] public int actionsLeft;
    [SerializeField] private int m_maxActionsLevel;
    [SerializeField] private CheckIntAmount m_consumeAction;
    
    [SerializeField] private CheckIntAmount m_checkActionAmount;
    [SerializeField] private ActionTimerView m_view;
    public event Action OnActionFinish;
    public event Action<int> OnActionChanged;

    private void Awake()
    {
        m_view = Instantiate(m_view,transform);
    }

    private void Start()
    {
        m_consumeAction.OnRequest += TryCostAction;
        m_checkActionAmount.OnRequest += Check;
         actionsLeft = m_maxActionsLevel;
         OnActionChanged?.Invoke(actionsLeft);
      
    }

    private bool Check(int cost)
    {
        return actionsLeft >= cost;
    }
    private bool TryCostAction(int cost)
    {
        if(actionsLeft < cost) return false; // si no hay suficiente actione
        actionsLeft -= cost; 
        OnActionChanged?.Invoke(actionsLeft);
        return true;
    }
    private void Finish()
    {
        OnActionFinish?.Invoke();
    }
    private void OnDestroy()
    {
        m_checkActionAmount.OnRequest -= TryCostAction;
    }
}

public interface IAction
{
    public int Cost { get ; set; }
    public void RequiredConsume();
}