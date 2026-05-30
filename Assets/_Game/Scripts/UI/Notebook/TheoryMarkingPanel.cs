using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using System;

public class TheoryMarkingPanel : Singleton<TheoryMarkingPanel>, IActivity
{
    [Header("Event")]
    [Header("Core")]
    [SerializeField] private EvidenceEvent m_evidenceEvent;
    [SerializeField] private Check m_checkEvidence;
    [SerializeField] private EventChannel m_refreshData;
    
    [Header("Activity")]
    [SerializeField] private IActivityEvent m_pushEvent;
    [SerializeField] private EventChannel m_popEvent;

    [Header("UI Setting")]
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private TMP_Text m_tipText;
    [SerializeField] private Button m_confirmButton, m_cancelButton;

    [SerializeField] private GameObject m_background;
    [SerializeField] private GameObject m_panel;
    
    [Header("Input")]
    [SerializeField] private MarkingInputReader m_inputReader;
    
    [Header("Data")]
    private readonly HashSet<SerializableGuid> _possibleEvidenceMarked = new();
    
    private Evidence _currentEvidenceOnEdit;
    private string _cachedRandomTip = string.Empty;

    private readonly Dictionary<PageType, List<string>> _tips = new()
    {
        { PageType.Character, new List<string>()
        {
            "e.g. 'X claims Y hates Z.'",
            "e.g. 'X heard Y talking with Z.'",
            "e.g. 'X thinks Y is hiding something.'",
            "e.g. 'X said they know about Y.'",
            "e.g. 'X avoided talking about Y.'",
            "e.g. 'X saw Y carrying Z.'",
        }},
        { PageType.Objects, new List<string>()
        {
            "e.g. 'X was used by Y.'",
            "e.g. 'X thinks this is dangerous.'",
            "e.g. 'X knows about this Y.'",
            "e.g. 'X might belong to Y.'",
            "e.g. 'X was given to Y by Z.'",
            "e.g. 'X gonna give it to Y.'",
        }},
    };
    
    #region Core Funtion

    private void Start()
    {
        m_evidenceEvent.OnEventRaised += MarkOrRemove;
        m_checkEvidence.OnRequest += Check;
        m_confirmButton.onClick.AddListener(() =>
        {
            if (_currentEvidenceOnEdit != null)
            {
                string defaultName = !string.IsNullOrEmpty(_cachedRandomTip) ? _cachedRandomTip : _currentEvidenceOnEdit.displayName;
                string newName = string.IsNullOrWhiteSpace(m_inputField.text) ? defaultName : m_inputField.text;
                RenameEvidence(_currentEvidenceOnEdit.guid, newName);
            }
            FoldPanel().Forget();
        });
        m_cancelButton.onClick.AddListener(() =>
        {
            FoldPanel().Forget();
            _currentEvidenceOnEdit = null;
        });
        
        m_background.SetActive(false);
    }

    private void OnDestroy()
    {
        m_evidenceEvent.OnEventRaised -= MarkOrRemove;
        m_checkEvidence.OnRequest -= Check;
        
        m_confirmButton.onClick.RemoveAllListeners();
        m_cancelButton.onClick.RemoveAllListeners();
    }

    private bool Check(SerializableGuid guid) => _possibleEvidenceMarked.Contains(guid);
    
    private void MarkOrRemove(Evidence evidence)
    {
        if (_possibleEvidenceMarked.Contains(evidence.guid))
        {
            _possibleEvidenceMarked.Remove(evidence.guid);
        }
        else
        {
            _currentEvidenceOnEdit = evidence;
            UnfoldPanel().Forget();
        }
    }

    #endregion

    #region Visual

    private async UniTask UnfoldPanel()
    {
        if (m_panel == null) return;
        
        SetRandomTipForCurrentEvidence();
        m_background.SetActive(true);
        Tween.StopAll(m_panel.gameObject.transform);
        var seq = Sequence.Create();
       
        m_panel.gameObject.transform.localScale = new Vector3(0, 1, 1);
        await seq.Group(Tween.ScaleX(m_panel.gameObject.transform, 1f, 0.3f, Ease.OutBack));
        await seq;
    }

    private void SetRandomTipForCurrentEvidence()
    {
        if (_currentEvidenceOnEdit == null) return;
        
        PageType currentType = GetPageTypeFromEvidence(_currentEvidenceOnEdit);

        if (!_tips.TryGetValue(currentType, out List<string> tipList) || tipList.Count <= 0) return;
        int randomIndex = UnityEngine.Random.Range(0, tipList.Count);
        _cachedRandomTip = tipList[randomIndex];

        if (m_inputField.placeholder != null && m_inputField.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = _cachedRandomTip;
        }
        
        if (m_tipText != null)
        {
            m_tipText.text = _cachedRandomTip;
        }
    }

    private PageType GetPageTypeFromEvidence(Evidence evidence)
    {
        if (evidence is DialogueFragmentNote) return PageType.Character;
        return PageType.Objects;
    }
    
    private async UniTask FoldPanel()
    {
        if (m_panel == null) return;
        Tween.StopAll(m_panel.gameObject.transform);

        var seq = Sequence.Create();
        await seq.Group(Tween.ScaleX(m_panel.gameObject.transform, 0f, 0.2f, Ease.InQuad));
        
        await seq;
        
        m_inputField.text = string.Empty;
        _cachedRandomTip = string.Empty;
        m_background.SetActive(false);
    }

    #endregion

    private void RenameEvidence(SerializableGuid guid, string newName)
    {
        if (!EvidenceDataBase.Instance.TryGet(guid, out var evidence)) return;

        evidence.displayName = newName;
        _possibleEvidenceMarked.Add(guid);
        m_refreshData?.Raise();
    }

    #region Activity

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    
    public void Resume() { }
    public void Pause() { }
    public void Stop() { }

    public bool CanPopWithKey()
    {
        return false;
    }
    
    #endregion
}