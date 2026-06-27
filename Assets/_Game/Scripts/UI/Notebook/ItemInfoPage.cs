using System;
using System.Collections.Generic;
using System.Threading; 
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ItemInfoPage : NotebookPage
{
    [SerializeField] private Transform root;
    [SerializeField] private ItemEventChannel m_requiredItemInfo;

    [SerializeField] private ImageSetting m_imageSetting;
    [SerializeField] private DynamicTextSetting m_description;
    private readonly List<IFlyweight> m_uiElements = new List<IFlyweight>();

    [SerializeField] private float size;
    [SerializeField] private Color color;
    [SerializeField] private float speed;
    
    [SerializeField]
    private float maxWidth = 120f;
   
    private CancellationTokenSource _textCancellationTokenSource;
    private DynamicUIText _currentActiveText;

    private void Awake()
    {
        m_requiredItemInfo.OnEventRaised += ShowItemInfo;
    }

    private void ShowItemInfo(Item item)
    {
        Despawn(); 

       
        var icon = FlyweightFactory.Instance.Spawn<UIImage>(m_imageSetting, Vector3.zero, Quaternion.identity, root);
        icon.SetImage(item.sprite);
        m_uiElements.Add(icon);
        
        string fullText = item.baseClue;
        var unlockedPois = NotebookManager.Instance.GetUnlockedPoiDescriptions(item);
        
        if (unlockedPois is { Count: > 0 })
        {
            fullText += "\n\nClue Founded : "; 
            for (int i = 0; i < unlockedPois.Count; i++)
            {
                fullText += $"\n{i + 1}) {unlockedPois[i]}";
            }
        }
        
        string flashback = NotebookManager.Instance.GetItemFlashbackInfo(item);
        if (!string.IsNullOrEmpty(flashback))
        {
            fullText += $"\n\nFlashback: {flashback}";
        }
        
        OnItemInfoTriggered(fullText).Forget();
        
        
    }

    private async UniTask OnItemInfoTriggered(string contentText)
    {
       
        CancelAndDisposeToken();
        _textCancellationTokenSource = new CancellationTokenSource();

      
        if (_currentActiveText != null)
        {
            FlyweightFactory.Instance.Return(_currentActiveText);
            _currentActiveText = null;
        }

        
        await PlayText(contentText, _textCancellationTokenSource.Token);
    }

  
    private async UniTask PlayText(string text, CancellationToken token) 
    {
        if (token.IsCancellationRequested) return;
        if (text == null) return;
        
      
        _currentActiveText = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_description, 
            Vector3.zero, 
            Quaternion.identity, 
            root
        );
        
      
       
        _currentActiveText.SetText(text, size, color,maxWidth: maxWidth);
        _currentActiveText.ToLast();

      
        await UniTask.NextFrame(token);
        
        try
        {
           
            await _currentActiveText.PlayTypeWriterEffect(externalToken: token);
        }
        catch (OperationCanceledException)
        {
           
        }
    }
    
    private void CancelAndDisposeToken()
    {
        if (_textCancellationTokenSource == null) return;
        _textCancellationTokenSource.Cancel();
        _textCancellationTokenSource.Dispose();
        _textCancellationTokenSource = null;
    }

    private void OnDestroy()
    {
        m_requiredItemInfo.OnEventRaised -= ShowItemInfo;
        
        CancelAndDisposeToken();
    }

    private void Despawn()
    {
        foreach (var uiElement in m_uiElements)
        {
            FlyweightFactory.Instance.Return(uiElement);
        }
        m_uiElements.Clear();
        
    }
}