using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionTimerView : MonoBehaviour
{
    [SerializeField] private TMP_Text m_text;
    public Transform UIParent;
    [SerializeField] private Image mainUI;
    [SerializeField] private Image shadeUI;
    [SerializeField] private float timeToFadeUI, timeToShowUI;
    [SerializeField] private BoolEventChannel m_showTimer; 
    private ActionTimer _actionTimer;
    private CancellationTokenSource _fadeCts;
    private bool _isForceShowing = false; 

    private void Awake()
    {
        _actionTimer = GetComponentInParent<ActionTimer>();
        
        
        SetAlpha(0f);
        UIParent.position -= new Vector3(0, UIManager.Instance.AspectRatioOffset(), 0);

        ShowCostLeft(_actionTimer.m_maxActionsLevel);
    }

    private void OnEnable()
    {
        _actionTimer.OnActionChanged += ShowCostLeft;
        
        if (m_showTimer != null)
        {
            m_showTimer.OnEventRaised += OnForceShowChanged;
        }
    }

    private void OnDisable()
    {
        _actionTimer.OnActionChanged -= ShowCostLeft;
        
        if (m_showTimer != null)
        {
            m_showTimer.OnEventRaised -= OnForceShowChanged;
        }
        CancelActiveFade();
    }

    private void ShowCostLeft(int left)
    {
        
        m_text.text = left.ToString();
        UIElement.CalculateWidthAndHeight(m_text, m_text.rectTransform);


        if (_isForceShowing) return;
        CancelActiveFade();
        _fadeCts = new CancellationTokenSource();
        _ = DisplayUI(_fadeCts.Token);
    }

    private void OnForceShowChanged(bool show)
    {
        _isForceShowing = show;
        CancelActiveFade();
        _fadeCts = new CancellationTokenSource();

        _ = show ? FadeIn(_fadeCts.Token) : FadeOut(_fadeCts.Token); 
    }


    public async UniTask FadeIn(CancellationToken token)
    {
        while (mainUI.color.a < 1f)
        {
            if (token.IsCancellationRequested) return;

            float step = 0.04f * (timeToFadeUI > 0 ? timeToFadeUI : 1f) / 5f;
            SetAlpha(Mathf.Min(1f, mainUI.color.a + step));
            
            await UniTask.Delay(20, cancellationToken: token).SuppressCancellationThrow();
        }
    }
    
    private async UniTask FadeOut(CancellationToken token)
    {
        while (mainUI.color.a > 0f)
        {
            if (token.IsCancellationRequested) return;

            float step = 0.03f * (timeToFadeUI > 0 ? timeToFadeUI : 1f) / 5f;
            SetAlpha(Mathf.Max(0f, mainUI.color.a - step));

            await UniTask.Delay(20, cancellationToken: token).SuppressCancellationThrow();
        }
    }

   
    private async UniTask DisplayUI(CancellationToken token)
    {
        // 淡入
        while (mainUI.color.a < 1f)
        {
            if (token.IsCancellationRequested) return;
            float step = 0.04f * timeToFadeUI / 5f;
            SetAlpha(Mathf.Min(1f, mainUI.color.a + step));
            await UniTask.Delay(20, cancellationToken: token).SuppressCancellationThrow();
        }

        // 停留
        bool canceled = await UniTask.Delay((int)(1000 * timeToShowUI), cancellationToken: token).SuppressCancellationThrow();
        if (canceled || token.IsCancellationRequested) return;

        // 淡出
        while (mainUI.color.a > 0f)
        {
            if (token.IsCancellationRequested) return;
            float step = 0.03f * timeToFadeUI / 5f;
            SetAlpha(Mathf.Max(0f, mainUI.color.a - step));
            await UniTask.Delay(20, cancellationToken: token).SuppressCancellationThrow();
        }
    }
    
    private void SetAlpha(float alpha)
    {
        mainUI.color = new Color(mainUI.color.r, mainUI.color.g, mainUI.color.b, alpha);
        m_text.color = new Color(m_text.color.r, m_text.color.g, m_text.color.b, alpha);
        shadeUI.color = new Color(shadeUI.color.r, shadeUI.color.g, shadeUI.color.b, alpha * 0.5f); // 保持阴影是主UI一半的比例
    }

    private void CancelActiveFade()
    {
        if (_fadeCts != null)
        {
            _fadeCts.Cancel();
            _fadeCts.Dispose();
            _fadeCts = null;
        }
    }
}