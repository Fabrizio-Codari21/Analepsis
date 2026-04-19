using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionTimerView : MonoBehaviour
{
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private Image mainUI;
    [SerializeField] private Image shadeUI;
    [SerializeField] private float timeToFadeUI, timeToShowUI;
    
    private ActionTimer _actionTimer;

    private void Awake()
    {
        _actionTimer = GetComponentInParent<ActionTimer>();
        shadeUI.color -= new Color(0, 0, 0, shadeUI.color.a);
        mainUI.color -= new Color(0, 0, 0, mainUI.color.a);
        m_text.color -= new Color(0, 0, 0, m_text.color.a);

        ShowCostLeft(_actionTimer.m_maxActionsLevel);
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
        m_text.text = left.ToString();
        UIElement.CalculateWidthAndHeight(m_text,m_text.rectTransform);
        _ = DisplayUI();
    }

    public async UniTask DisplayUI()
    {

        while(mainUI.color.a < 1)
        {
            mainUI.color += new Color(0, 0, 0, 0.04f * timeToFadeUI/5);
            m_text.color += new Color(0, 0, 0, 0.04f * timeToFadeUI/5);
            shadeUI.color += new Color(0, 0, 0, 0.02f * timeToFadeUI/5);
            await UniTask.Delay(20);
        }

        await UniTask.Delay((int)(1000*timeToShowUI));

        while (mainUI.color.a > 0)
        {
            mainUI.color -= new Color(0, 0, 0, 0.03f * timeToFadeUI / 5);
            m_text.color -= new Color(0, 0, 0, 0.03f * timeToFadeUI / 5);
            shadeUI.color -= new Color(0, 0, 0, 0.015f * timeToFadeUI / 5);
            await UniTask.Delay(20);
        }

    }
}