using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Cinemachine;
public class TheoryboardView : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera m_camera;
    
    [Header("UI")]
    [SerializeField] private Button m_solveButton;
    [SerializeField] private TMP_Text m_solveText;
    [SerializeField] private Button m_previousCharacterButton, m_nextCharacterButton;
    
    [Header("Dynamic Slots Config")]
    [SerializeField] private TheorySlot m_slotPrefab;
    [SerializeField] private Transform m_slotGridRoot;
    private readonly List<TheorySlot> _allRuntimeSlots = new List<TheorySlot>();
    
    [Header("UI References (Roots)")]
    [Space(10)]
    [SerializeField] private EvidenceDataSlots m_logBackpackSlot;
    [SerializeField] private EvidenceDataSlots m_itemSlots;
    [SerializeField] private CharacterSlot m_characterSlot;
    [ShowInInspector,ReadOnly]private int _currentCharacterIndex = 0;
    [ShowInInspector,ReadOnly]private readonly List<NpcIdentity> _cachedFoundCharacters = new List<NpcIdentity>();
    [SerializeField] private Transform m_itemRoot;

    
    [Header("Extra UI")]
    [SerializeField] private FullScreenTipUI m_erroTip;

    [Header("Events")]
    [SerializeField] private EventChannel m_solverChannel;

    private IActivity _activity;

    #region Unity Life

    private void Start()
    {
        #region Activity Subscribe  
        _activity = GetComponentInParent<IActivity>();

        _activity.OnResume += () =>
        {
            
            m_camera.enabled = true;
            LoadMarkedClues();
            RefreshCharacterLayout();
        };
        _activity.OnStop += () =>
        {
            m_camera.enabled = false;
            
            ResetAllSlotsData();                                       
        };
        #endregion
        
        if (m_erroTip != null)
        {
            m_erroTip = Instantiate(m_erroTip, transform);
        }
    }

    private void OnEnable()
    {
        if (m_solveButton != null && m_solverChannel != null)
        {
            m_solveButton.onClick.AddListener(() => m_solverChannel.Raise());
        }
        if (m_previousCharacterButton != null) m_previousCharacterButton.onClick.AddListener(() => SwitchCharacter(-1));
        if (m_nextCharacterButton != null) m_nextCharacterButton.onClick.AddListener(() => SwitchCharacter(1));
    }

    private void OnDisable()
    {
        if (m_solveButton != null) m_solveButton.onClick.RemoveAllListeners();
        if (m_previousCharacterButton != null) m_previousCharacterButton.onClick.RemoveAllListeners();
        if (m_nextCharacterButton != null) m_nextCharacterButton.onClick.RemoveAllListeners();
    }

    #endregion

    public List<TheorySlot> InitializeBoardArchitecture(CaseResolution caseResolution)
    {
        if (caseResolution == null || caseResolution.allSlots == null) 
            return _allRuntimeSlots;

        var targetIdentities = caseResolution.allSlots;

        foreach (var t in targetIdentities)
        {
            if (t == null) continue;
            if (m_slotPrefab == null || m_slotGridRoot == null) break;

            TheorySlot newSlotInstance = Instantiate(m_slotPrefab, m_slotGridRoot);
            newSlotInstance.SetIdentity(t);
            newSlotInstance.gameObject.SetActive(true);
            _allRuntimeSlots.Add(newSlotInstance);
        }

        return _allRuntimeSlots; 
    }

    private void ResetAllSlotsData()
    {
        m_logBackpackSlot.ClearSlot(); 
        m_characterSlot.ClearSlot();
        m_itemSlots.ClearSlot();
    }

    public void LoadMarkedClues() 
    {
        if (m_logBackpackSlot == null) return;

      
        m_logBackpackSlot.ClearSlot();
        
        var allMarked = TheoryMarkingPanel.Instance.MarkedEvidences;
        var enumerable = allMarked.ToList();
        var markedLogs = enumerable.Where(e => e is DialogueFragmentNote).ToList();
        var markedItem = enumerable.Where(e => e is ItemEvidence) .ToList();
        if (markedLogs.Count > 0)
        {
            
            foreach (var log in markedLogs) m_logBackpackSlot.ReplaceData(log);
        }

        if (markedItem.Count > 0)
        {
            foreach (var item in markedItem) m_itemSlots.ReplaceData(item);
        }
      
        
        
    }
    
    public async UniTask Tip(string solveTxt)
    {
        if (m_erroTip == null) return;
        m_erroTip.gameObject.SetActive(true);
        await m_erroTip.FadeInAndFadeOut(solveTxt);
        m_erroTip.gameObject.SetActive(false);
    }
    

    private void RefreshCharacterLayout()
    {
        _cachedFoundCharacters.Clear();
      
        _cachedFoundCharacters.AddRange(NotebookManager.Instance.FoundCharacters.Where(c => c != null));

        _currentCharacterIndex = 0;
        UpdateCharacterSlotDisplay();
    }
    
    private void SwitchCharacter(int direction)
    {
        if (_cachedFoundCharacters.Count <= 0) return;

        _currentCharacterIndex += direction;
        
        if (_currentCharacterIndex >= _cachedFoundCharacters.Count) _currentCharacterIndex = 0;
        if (_currentCharacterIndex < 0) _currentCharacterIndex = _cachedFoundCharacters.Count - 1;

        UpdateCharacterSlotDisplay();
    }
    
    
    private void UpdateCharacterSlotDisplay()
    {
        if (m_characterSlot == null) return;

        if (_cachedFoundCharacters.Count <= 0)
        {
            m_characterSlot.DisplayCharacter(null);
            return;
        }
        
        NpcIdentity currentNpc = _cachedFoundCharacters[_currentCharacterIndex];
        
        Debug.Log(currentNpc.npcName + " Guid " + currentNpc.npcGuid.GetHashCode());
        
        Evidence npcEvidence = EvidenceDataBase.Instance.GetOrCreate(currentNpc.npcGuid, () => new NpcEvidence(currentNpc.npcName, currentNpc.npcGuid, currentNpc.possibleRoles, currentNpc));
        
        m_characterSlot.DisplayCharacter(npcEvidence);
    }
}