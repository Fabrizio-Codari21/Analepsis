using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DynamicText : FactoryUIObject
{
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private float m_charsPerSecond = 30f;
    private CancellationTokenSource _cts;
    public override void OnSpawn()
    {
       base.OnSpawn();
       Cancel();
    }
    public void SetText(string text)
    {
        m_text.text = text;
        m_text.maxVisibleCharacters = 0;
    }
    public async UniTask PlayTypeWriterEffect(string text = null)
    {
        Cancel();
        m_text.text = text ?? string.Empty;
        _cts = new CancellationTokenSource();
        int totalChars = m_text.text.Length;
        try
        {
            for (int i = 1; i <= totalChars; i++)
            {
                m_text.maxVisibleCharacters = i;
                int delay = Mathf.RoundToInt(1000f / m_charsPerSecond);
                await UniTask.Delay(delay, cancellationToken: _cts.Token);
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