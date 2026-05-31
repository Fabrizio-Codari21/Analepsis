using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using TMPro;
using System;
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
    
    [Header("UI References")]
    [Space(10)]
    [Header("Root")]
    [SerializeField] private Transform m_logRoot;
    [SerializeField] private Transform m_itemRoot;
    [SerializeField] private Transform m_charactersRoot;
    
    [Header("Flyweight")]
    [SerializeField] private ButtonSetting m_buttonSetting;
    
    
    private readonly List<IFlyweight> _flyweights = new List<IFlyweight>();
    
    [Header("Extra UI")]
    [SerializeField] private FullScreenTipUI m_erroTip;

    [Header("Events")]
    [SerializeField] private EventChannel m_solverChannel;


    private IActivity _activity;

    #region Unity Life

    

    private void Start()
    {
        
        #region Activity Suscribe  
        _activity = GetComponentInParent<IActivity>();

        _activity.OnResume += () =>
        {
            m_camera.enabled = true;
        };
        _activity.OnStop += () =>
        {
            m_camera.enabled = false;
            Despawn();
            ResetAllSlotsData();
        };
        #endregion
        
        m_erroTip = Instantiate(m_erroTip,transform);
        
    }

    private void OnEnable()
    {
        // m_previousCharacterButton.onClick.AddListener(() => SwitchCharacter(-1));
        // m_nextCharacterButton.onClick.AddListener(() => SwitchCharacter(1));
        m_solveButton.onClick.AddListener(() =>  m_solverChannel.Raise());
    }

    private void OnDisable()
    {
        m_previousCharacterButton.onClick.RemoveAllListeners();
        m_nextCharacterButton.onClick.RemoveAllListeners();
        m_solveButton.onClick.RemoveAllListeners();
    }

    #endregion


    public List<TheorySlot> InitializeBoardArchitecture(CaseResolution caseResolution)
    {
        // 清防空机制
        if (caseResolution == null || caseResolution.allSlots == null) 
            return _allRuntimeSlots;

        var targetIdentities = caseResolution.allSlots;

        // 核心：如果有 5 个身份框，我就强行 Instantiate 生成 5 个常驻物理物体
        foreach (var t in targetIdentities)
        {
            var identityAsset = t;
            if (identityAsset == null) continue;

            if (m_slotPrefab == null || m_slotGridRoot == null)
            {
                Debug.LogError("【View 架构中止】未指定 m_slotPrefab 或 m_slotGridRoot 容器！");
                break;
            }

            // 🚀 动态物理克隆，并且立刻摆进 Grid
            TheorySlot newSlotInstance = Instantiate(m_slotPrefab, m_slotGridRoot);
            
            // 🔥 注入黑盒格子所需的灵魂身份证卡片，并在内部重命名
            newSlotInstance.SetIdentity(t);
            
            // 开启显示，并打入常驻列表
            newSlotInstance.gameObject.SetActive(true);
            _allRuntimeSlots.Add(newSlotInstance);
        }

        return _allRuntimeSlots; // 扔回给总管，总管也只需要接一次，永久享用
    }

    private void ResetAllSlotsData()
    {
        foreach (var slot in _allRuntimeSlots.Where(slot => slot != null))
        {
            slot.SetEvidence(null); 
            
            EvidenceRepresentButton residualButton = slot.GetComponentInChildren<EvidenceRepresentButton>();
            if (residualButton == null) continue;
            if (_flyweights.Contains(residualButton))
            {
                _flyweights.Remove(residualButton);
            }
              
            FlyweightFactory.Instance.Return(residualButton);
        }
    }
    int _currentCharacter = 0;
    public void LoadMarkedClues() 
    {

        Despawn();
        var allMarked = TheoryMarkingPanel.Instance.MarkedEvidences;
        
        var markedLogs = allMarked.Where(e => e is DialogueFragmentNote).ToList();

        if (markedLogs.Count <= 0) return;
        foreach (var log in markedLogs)
        {
            var button = CreateClueButton(log.displayName, m_logRoot,log); 
            _flyweights.Add(button);
        }

    }

    private void SwitchCharacter(int nextOrPrevious = 0)
    {
        // Despawn(m_charactersRoot);
        if(NotebookManager.Instance.FoundCharacters.Count <= 0)
        {
            CreateClueButton("No characters discovered.", m_charactersRoot, null); 
            return;
        }
        
        var character = NotebookManager.Instance.FoundCharacters.ToList()[_currentCharacter];
        
        switch (nextOrPrevious)
        {
            case > 0:
            {
                _currentCharacter++;
                if (_currentCharacter > NotebookManager.Instance.FoundCharacters.Count - 1) _currentCharacter = 0;
                break;
            }
            case < 0:
            {
                _currentCharacter--;
                if (_currentCharacter < 0) _currentCharacter = NotebookManager.Instance.FoundCharacters.Count - 1;
                break;
            }
        }
        // CreateClueButton(character.npcName, m_charactersRoot, new(character,new List<Whodunnit>(character.possibleRoles)), isCharacter: true);
    }

    private ButtonFactoryObject CreateClueButton(string text, Transform parent,Evidence evidence)
    {
        var button = FlyweightFactory.Instance.Spawn<EvidenceRepresentButton>(
            m_buttonSetting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );
        
        button.SetEvidence(evidence);
        button.SetText(text);
        button.SetInteractable(true);
        button.MoveToLast();
        return button;
    }

    private void Despawn() 
    {
        foreach (var f in _flyweights)
        {
            if (f != null)FlyweightFactory.Instance.Return(f);
        }
        _flyweights.Clear();    
    }
    
    public async UniTask ShowError(string solveTxt)
    {
        // var oldText = solveText.text;
        //
        // print("unsolved");
        // //solveText.text = "Not quite";
        // solveText.text = $"Wait... this doesn't make enough sense. " +
        //                  $"\n I need to find a better theory, quickly... " + $"\n [{manager.AttemptsLeft} attempts left]";
        // await DisplayErrorUI();
        // solveText.text = oldText;
    }

    public async UniTask Tip(string solveTxt)
    {
        m_erroTip.gameObject.SetActive(true);
        await m_erroTip.FadeInAndFadeOut(solveTxt);
        m_erroTip.gameObject.SetActive(false);
    }


    public void CreateSlot(CaseSlot slot)
    {
        
    }



}