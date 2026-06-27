using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterNotebookPage : NotebookPage
{
    [Header("Character Info")]
    [SerializeField] private Image m_characterIcon;
    [SerializeField] private TMP_Text m_text;

    [SerializeField] private GameObject m_characterSelectionPanel;
    [SerializeField] private Button m_panelUnfoldButton;
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

    private void OnEnable()
    {
        m_panelUnfoldButton.onClick.AddListener(() =>
        {
            if (m_characterSelectionPanel.activeSelf)
            {
                FoldPanel().Forget();
            }
            else
            {
                UnfoldPanel().Forget();
            }
        });
    }

    private void OnDisable()
    {
        m_characterSelectionPanel.gameObject.SetActive(false);
        m_panelUnfoldButton.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        m_onCharacterSelected.OnEventRaised -= SwitchCharacter;
        m_onNpcAdded.OnEventRaised -= AddNpc;
        m_receiveNodeInfo.OnEventRaised -= ShowInfo;
    }
    
    private void Update()
    {
  
        if (!m_characterSelectionPanel.activeSelf) return;


        if (!Input.GetMouseButtonDown(0)) return;
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            
        if (currentSelected == null || !IsPointerOverPanel(currentSelected))
        {
            if (currentSelected != m_panelUnfoldButton.gameObject)
            {
                FoldPanel().Forget();
            }
        }
    }

    private bool IsPointerOverPanel(GameObject clickedObject)
    {
        Transform current = clickedObject.transform;
        while (current != null)
        {
            if (current.gameObject == m_characterSelectionPanel)
                return true;
            
            current = current.parent;
        }
        return false;
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
        PlayText(info, token: _textCancellationTokenSource.Token, sizeOverride: m_textSize).Forget();
    }

    private void CreateButtonInstance(NpcIdentity npc)
    {
        var buttonInstance = Instantiate(m_button, m_buttonRoot);
        buttonInstance.Init(npc);
        buttonInstance.AddListener(() =>
        {
            FoldPanel().Forget();
        });
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
    
    private async UniTask UnfoldPanel()
    {
        m_characterSelectionPanel.SetActive(true);
        Tween.StopAll(m_characterSelectionPanel.gameObject.transform);
        var seq = Sequence.Create();
       
        m_characterSelectionPanel.gameObject.transform.localScale = new Vector3(0, 1, 1);
        await seq.Group(Tween.ScaleX(m_characterSelectionPanel.gameObject.transform, 1f, 0.3f, Ease.OutBack));
        await seq;
    }
    
    private async UniTask FoldPanel()
    {
        if (m_characterSelectionPanel == null) return;
        Tween.StopAll(m_characterSelectionPanel.gameObject.transform);

        var seq = Sequence.Create();
        await seq.Group(Tween.ScaleX(m_characterSelectionPanel.gameObject.transform, 0f, 0.2f, Ease.InQuad));
        await seq;
        
        m_characterSelectionPanel.SetActive(false);
    }
}