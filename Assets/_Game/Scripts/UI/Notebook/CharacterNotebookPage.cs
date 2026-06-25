using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterNotebookPage : NotebookPage
{
    [Header("Character Info")]
    [SerializeField] private Image m_characterIcon;
    [SerializeField] private TMP_Text m_text;

    [Header("Button Setting")]
    [SerializeField] private Transform m_buttonRoot;
    [SerializeField] private CharacterSwitchButton m_button;
    
    [Header("Event")]
    [SerializeField] private NpcEvent m_onCharacterSelected;
    [SerializeField] private NpcEvent m_onNpcAdded;
    [SerializeField] private StringEventChannel m_receiveNodeInfo;
    
    private readonly HashSet<NpcIdentity> _instantiatedButtons = new();
    
    
    [Header("Text")]
    [SerializeField] private DynamicTextSetting m_dynamicTextSetting;
    [SerializeField] private ScrollRect m_scrollRect;
    [SerializeField] private Transform m_textRoot;
    [SerializeField] private float m_textWidth = 150f;
    [SerializeField] private float m_textSize = 12f;
    
    
    private CancellationTokenSource _textCancellationTokenSource;
    private DynamicUIText _currentActiveText;
    private void Start()
    {
        m_onCharacterSelected.OnEventRaised += SwitchCharacter;
        m_onNpcAdded.OnEventRaised += AddNpc;
        m_receiveNodeInfo.OnEventRaised += ShowInfo;
    }

    private void OnDestroy()
    {
        m_onCharacterSelected.OnEventRaised -= SwitchCharacter;
        m_onNpcAdded.OnEventRaised -= AddNpc;
        m_receiveNodeInfo.OnEventRaised -= ShowInfo;
    }

    private void SwitchCharacter(NpcIdentity key)
    {
        if (key == null) return;
        m_characterIcon.gameObject.SetActive(true);
        m_text.gameObject.SetActive(true);
        m_characterIcon.sprite = key.filePhoto;
        m_text.text = key.characterInfo;
    }


    public void SyncAllButtons(List<NpcIdentity> currentCharacters)
    {
        if (currentCharacters == null) return;

        foreach (var npc in currentCharacters)
        {
            if (npc != null && !_instantiatedButtons.Contains(npc))
            {
                CreateButtonInstance(npc);
            }
        }
    }

    public void SetupPage(NpcIdentity currentNpc)
    {
        SwitchCharacter(currentNpc);
    }

    private void AddNpc(NpcIdentity newNpc)
    {
        if (!newNpc) return;
        if (_instantiatedButtons.Contains(newNpc)) return;
        CreateButtonInstance(newNpc);
    }

    private void ShowInfo(string info)
    {
        CancelAndDisposeToken();
        _textCancellationTokenSource = new CancellationTokenSource();
        PlayText(info,token: _textCancellationTokenSource.Token, sizeOverride : m_textSize).Forget();
    }

  
    private void CreateButtonInstance(NpcIdentity npc)
    {
        var buttonInstance = Instantiate(m_button, m_buttonRoot);
        buttonInstance.Init(npc);
        _instantiatedButtons.Add(npc);
    }
    
    private void CancelAndDisposeToken()
    {
        if (_textCancellationTokenSource == null) return;
        _textCancellationTokenSource.Cancel();
        _textCancellationTokenSource.Dispose();
        _textCancellationTokenSource = null;
    }

  
    private async UniTask PlayText(string text, CancellationToken token, Transform parent = null, float sizeOverride = 0) 
    {
        if (token.IsCancellationRequested) return;
        if (text == null) return;
        
        if(_currentActiveText != null)
        {
            FlyweightFactory.Instance.Return(_currentActiveText);
        }
        
        _currentActiveText = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dynamicTextSetting, 
            Vector3.zero, 
            Quaternion.identity, 
            parent != null ? parent : m_textRoot
        );
        
        _currentActiveText.SetText(text, !Mathf.Approximately(sizeOverride, 0) ? sizeOverride : m_dynamicTextSetting.size, m_dynamicTextSetting.color, m_textWidth, true);
        
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

}