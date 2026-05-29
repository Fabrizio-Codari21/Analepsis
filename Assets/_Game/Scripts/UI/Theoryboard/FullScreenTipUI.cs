using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;

public class FullScreenTipUI : MonoBehaviour
{
    [Header("Setting")]
    [Header("UI")]
    [SerializeField] private CanvasGroup m_solveCanvas;
    [SerializeField] private TMP_Text m_text;
    [Header("Fade ")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float stayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Property")]
    [SerializeField] private bool m_startEnable;

    private Sequence _currentSequence;

    private void Start()
    {
        gameObject.SetActive(m_startEnable);
    }

    public async UniTask FadeInAndFadeOut(string text)
    {
        _currentSequence.Stop();
        m_text.text = text;
        gameObject.SetActive(true);
        
        m_solveCanvas.alpha = 0f;
        _currentSequence = Sequence.Create()
            
            .Chain(Tween.Alpha(m_solveCanvas, 0.8f, fadeInDuration, Ease.OutQuad))
            
            .ChainDelay(stayDuration)
            
            .Chain(Tween.Alpha(m_solveCanvas, 0f, fadeOutDuration, Ease.InQuad));

        await _currentSequence;

        gameObject.SetActive(false);
    }
}