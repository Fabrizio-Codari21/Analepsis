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
     private float m_maxWidth = 500f;
    bool _setToMaxWidth = false;
    private CancellationTokenSource _cts;

    [Button]
    public void CalculateWidthAndHeight()
    {
        Vector2 prefSize = m_text.GetPreferredValues(m_text.text);
        
        float finalWidth;
        float finalHeight;

        if (prefSize.x > m_maxWidth || _setToMaxWidth)
        {
            finalWidth = m_maxWidth;
            finalHeight = m_text.GetPreferredValues(m_text.text, m_maxWidth, 0).y;
        }
        else
        {
            finalWidth = prefSize.x;
            finalHeight = prefSize.y;
        }
        m_rectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
        m_rectTransform.anchoredPosition = Vector2.zero;
    }
    public override void OnSpawn()
    {
       base.OnSpawn();
       Cancel();
    }
    public void SetText(string text,float size, Color color,float maxWidth = 500f, bool setToMaxWidth = false)
    {
        m_text.text = text;
        m_text.color = color;
        m_text.fontSize = size;
        m_text.maxVisibleCharacters = 0; 
        m_maxWidth =  maxWidth;
        _setToMaxWidth = setToMaxWidth;
        CalculateWidthAndHeight();
    }

    public void ToLast()
    {
        transform.SetAsLastSibling();
    }
    
  
    public async UniTask PlayTypeWriterEffect(string text = null,CancellationToken externalToken = default, float typingSpeed = default)
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
        float typeSpeed = typingSpeed != default ? typingSpeed : m_charsPerSecond;
        try
        {
            for (int i = 1; i <= totalChars; i++)
            {
                token.ThrowIfCancellationRequested();

                m_text.maxVisibleCharacters = i;

                await UniTask.Delay(
                    TimeSpan.FromSeconds(1f / typeSpeed),
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