using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private Transform m_itemRoot;
    [SerializeField] private Transform m_charactersRoot;
    
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
        foreach (var slot in _allRuntimeSlots)
        {
            if (slot != null) slot.ClearSlot();
        }
        
        m_logBackpackSlot.ClearSlot();
    }

    public void LoadMarkedClues() 
    {
        if (m_logBackpackSlot == null) return;

      
        m_logBackpackSlot.ClearSlot();
        
        var allMarked = TheoryMarkingPanel.Instance.MarkedEvidences;
        var markedLogs = allMarked.Where(e => e is DialogueFragmentNote).ToList();
        if (markedLogs.Count <= 0) return;

      
        foreach (var log in markedLogs) m_logBackpackSlot.ReplaceData(log);
        
    }
    
    public async UniTask Tip(string solveTxt)
    {
        if (m_erroTip == null) return;
        m_erroTip.gameObject.SetActive(true);
        await m_erroTip.FadeInAndFadeOut(solveTxt);
        m_erroTip.gameObject.SetActive(false);
    }
}