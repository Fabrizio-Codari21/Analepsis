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
 
    
    
    [Header("UI References")]
    [Space(10)]
    [Header("Root")]
    [SerializeField] private Transform m_logRoot;
    [SerializeField] private Transform m_itemRoot;
    [SerializeField] private Transform m_charactersRoot;
    
    [Header("Flyweight")]
    [SerializeField] private ButtonSetting m_buttonSetting;
    
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
            Despawn(m_logRoot);
            Despawn(m_itemRoot);
        };
        #endregion
        
        m_erroTip = Instantiate(m_erroTip,transform);
        
    }

    private void OnEnable()
    {
        m_previousCharacterButton.onClick.AddListener(() => SwitchCharacter(-1));
        m_nextCharacterButton.onClick.AddListener(() => SwitchCharacter(1));
        m_solveButton.onClick.AddListener(() =>  m_solverChannel.Raise());
    }

    private void OnDisable()
    {
        m_previousCharacterButton.onClick.RemoveAllListeners();
        m_nextCharacterButton.onClick.RemoveAllListeners();
        m_solveButton.onClick.RemoveAllListeners();
    }

    #endregion


    int _currentCharacter = 0;
    public void LoadMarkedClues()  // Load 从 note manager 已经登记的 在 evidence 有的 
    {

        Despawn(m_logRoot);
        Despawn(m_itemRoot);
        var allMarked = TheoryMarkingPanel.Instance.MarkedEvidences;
        
        var markedLogs = allMarked.Where(e => e is DialogueFragmentNote).ToList();
        
        if (markedLogs.Count == 0)
        {
            CreateClueButton("No Logs marked \n(Click the star to mark)", m_logRoot, null, true);
        }
        else
        {
            foreach (var log in markedLogs)
            {
                CreateClueButton(log.displayName, m_logRoot, null); 
            }
        }
    }

    private void SwitchCharacter(int nextOrPrevious = 0)
    {
        Despawn(m_charactersRoot);
        if(NotebookManager.Instance.FoundCharacters.Count <= 0)
        {
            CreateClueButton("No characters discovered.", m_charactersRoot, null, true); 
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
        CreateClueButton(character.npcName, m_charactersRoot, new(character,new List<Whodunnit>(character.possibleRoles)), isCharacter: true);
    }

    private ButtonFactoryObject CreateClueButton(string text, Transform parent, Tuple<Clue,List<Whodunnit>> proof, bool placeholder = false, bool isCharacter = false)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            m_buttonSetting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );
        
        button.SetText(text);
        if (placeholder) 
        {
            button.SetInteractable(false);
            return button;
        }
        button.SetInteractable(true);
        button.MoveToLast();
        return null;
    }

    private void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
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



}