using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class DynamicUIText : FactoryUIObject
{
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private float m_charsPerSecond = 30f;
    private CancellationTokenSource _cts;

    [Button]
    public void CalculateWidthAndHeight()
    {
        Vector2 prefSize = m_text.GetPreferredValues(m_text.text);
        m_rectTransform.sizeDelta = new Vector2(prefSize.x, prefSize.y);
        m_rectTransform.anchoredPosition = new Vector2(0, 0);
    }
    public override void OnSpawn()
    {
       base.OnSpawn();
       Cancel();
    }
    public void SetText(string text,float size, Color color)
    {
        m_text.text = text;
        m_text.color = color;
        m_text.fontSize = size;
        m_text.maxVisibleCharacters = 0; 
        
        Vector2 prefSize = m_text.GetPreferredValues(m_text.text);
        m_rectTransform.sizeDelta = new Vector2(prefSize.x, prefSize.y);
        m_rectTransform.anchoredPosition = new Vector2(0, 0);
    }
    
  
    public async UniTask PlayTypeWriterEffect(string text = null,CancellationToken externalToken = default)
    {
        Cancel();

        if (text != null)
            m_text.text = text;

        await UniTask.Yield();
        m_text.maxVisibleCharacters = 0;
        _cts = new CancellationTokenSource();
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, externalToken);
        var token = linkedCts.Token;
        int totalChars = m_text.textInfo.characterCount;
        try
        {
            for (int i = 1; i <= totalChars; i++)
            {
                token.ThrowIfCancellationRequested();

                m_text.maxVisibleCharacters = i;

                await UniTask.Delay(
                    TimeSpan.FromSeconds(1f / m_charsPerSecond),
                    cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            m_text.maxVisibleCharacters = totalChars;
        }
    }

    private void Cancel()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }
    
}