using System;
using TMPro;
using UnityEngine;

public class ActionTimerView : MonoBehaviour
{
    [SerializeField] private TMP_Text m_text;
    
    private ActionTimer _actionTimer;

    private void Awake()
    {
        _actionTimer = GetComponentInParent<ActionTimer>();
        
    }

    private void OnEnable()
    {
        _actionTimer.OnActionChanged += ShowCostLeft;
    }

    private void OnDisable()
    {
        _actionTimer.OnActionChanged -= ShowCostLeft;
    }


    private void ShowCostLeft(int left)
    {
        m_text.text =  "Cost Left : "+ left ;
        UIElement.CalculateWidthAndHeight(m_text,m_text.rectTransform);
    }
}