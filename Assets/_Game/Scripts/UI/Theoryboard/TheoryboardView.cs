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
    public void LoadMarkedClues()
    {

        Despawn(m_logRoot);
        Despawn(m_itemRoot);
        var markedLogs = TheoryMarkingPanel.Instance.MarkedClues.Where(x => x.Value.type == PageType.Character);
        var markedList = markedLogs.ToList();
        if(!markedList.Any()) CreateClueButton("No Logs marked \n(Click the star to mark)", m_logRoot, null, true);
        var markedItems = TheoryMarkingPanel.Instance.MarkedClues.Where(x => x.Value.type == PageType.Objects);
        var keyValuePairs = markedItems.ToList();
        if (!keyValuePairs.Any()) CreateClueButton("No Objects marked \n(Click the star to mark)", m_itemRoot, null, true);
        
        SwitchCharacter();
        
        if ( TheoryMarkingPanel.Instance.MarkedClues.Count <= 0) return;
        foreach (var log in markedList) 
        {
            CreateClueButton(log.Value.displayName, m_logRoot, log.Value.IsProof);
           
        }
        foreach (var item in keyValuePairs)
        {
           CreateClueButton(item.Value.displayName, m_itemRoot, item.Value.IsProof);
          
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

    public ButtonFactoryObject CreateClueButton(string text, Transform parent, Tuple<Clue,List<Whodunnit>> proof, bool placeholder = false, bool isCharacter = false)
    {
        // var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
        //     clueButtonSetting,
        //     Vector3.zero,
        //     Quaternion.identity,
        //     parent
        // );
        //
        // button.SetText(text);
        // if (placeholder) 
        // {
        //     button.SetInteractable(false);
        //     return button;
        // }
        // button.SetInteractable(true);
        // button.SetBoard(boardRoots);
        // button.SetView(this);
        // button.SetProof(proof);
        // button.SetCharacter(isCharacter);
        // button.MoveToLast();
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