using System;
using Sirenix.OdinInspector;
using UnityEngine;
public class ActionTimer : MonoBehaviour
{
    [ShowInInspector,ReadOnly] private int _actionLeft;
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
        
         _actionLeft = m_maxActionsLevel;
         OnActionChanged?.Invoke(_actionLeft);
      
    }


    private bool Check(int cost)
    {
        return _actionLeft >= cost;
    }
    private bool TryCostAction(int cost)
    {
        if(_actionLeft < cost) return false; // si no hay suficiente actione
        _actionLeft -= cost; 
        OnActionChanged?.Invoke(_actionLeft);
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