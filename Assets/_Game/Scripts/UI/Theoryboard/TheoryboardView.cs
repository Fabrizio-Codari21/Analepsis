using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.EventSystems;

public class TheoryboardView : MonoBehaviour
{
    [Header("Clue Setting")]
    public Transform markedLogsRoot;
    public Transform markedItemsRoot;
    public Transform markedCharactersRoot;
    public Button previousCharacterButton, nextCharacterButton;
    
    
    public ButtonSetting clueButtonSetting;
    
    public Button solveButton;

    public SerializedDictionary<Proof, TheoryPanel> boardRoots = new();

    int _currentCharacter = 0;
    
    [Header("Screen Setting")]
    [SerializeField] private CinemachineCamera m_viewCamera;
    private IActivity _activity;
    
    [SerializeField] private Canvas m_theoryCanvas;
    
    #region Theory board Request Event
    public event Action OnRequestSolved =  delegate { };
 
    #endregion
    
    
    #region Unity Life

    private void Awake()
    {
        m_viewCamera.enabled = false;
    }

    void Start()
    {
        solveButton.onClick.AddListener(() =>
        {
            OnRequestSolved?.Invoke();
        });
        _activity = GetComponentInParent<IActivity>();
        _activity.OnResume += ViewStart;
        _activity.OnStop += ViewEnd;
    }


    private void OnDestroy()
    {
        _activity.OnResume -= ViewStart;
        _activity.OnStop -= ViewEnd;
    }

    #endregion

    #region  Screen
    
    private void ViewStart()
    {
        m_viewCamera.enabled = true; // activar la camara de theory
    }
    
    private void ViewEnd()
    {
        m_viewCamera.enabled = false; // desactivar la camara de theroy
        Despawn(markedLogsRoot);
        Despawn(markedItemsRoot);
    }
    
    #endregion
    
    public void LoadMarkedClues()
    {
        // var markedLogs = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Log);
        // if(markedLogs.Count() <= 0)
        //     CreateClueButton("No Logs marked \n(Click the star to mark)", markedLogsRoot, default, true);
        // var markedItems = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Objects);
        // if (markedItems.Count() <= 0)
        //     CreateClueButton("No Objects marked \n(Click the star to mark)", markedItemsRoot, default, true);
        //
        // SwitchCharacter();
        //
        // if (notebookManager.markedClues.Count <= 0) return;
        //
        // foreach (var log in markedLogs) 
        // {
        //     var button = CreateClueButton(log.Value.displayName, markedLogsRoot, log.Value.isProof);
        //    
        // }
        // foreach (var item in markedItems)
        // {
        //     var button = CreateClueButton(item.Value.displayName, markedItemsRoot, item.Value.isProof);
        //
        // }

    }
    
    // private void SwitchCharacter(int nextOrPrevious = 0)
    // {
    //     Despawn(markedCharactersRoot);
    //     if(NotebookManager.Instance.FoundCharacters.Count <= 0)
    //     {
    //         CreateClueButton("No characters discovered.", markedCharactersRoot, null, true); return;
    //     }
    //     var character = NotebookManager.Instance.FoundCharacters.Keys.ToList()[_currentCharacter];
    //     if (nextOrPrevious > 0)
    //     {
    //         _currentCharacter++;
    //         if (_currentCharacter > NotebookManager.Instance.FoundCharacters.Count - 1) _currentCharacter = 0;
    //     }
    //     else if (nextOrPrevious < 0)
    //     {
    //         _currentCharacter--;
    //         if (_currentCharacter < 0) _currentCharacter = NotebookManager.Instance.FoundCharacters.Count - 1;
    //     }
    //     var charButton = CreateClueButton(character.npcName, markedCharactersRoot, new List<Proof>(){character.role}, isCharacter: true);
    // }

    public ButtonFactoryObject CreateClueButton(string text, Transform parent, List<Proof> proof, bool placeholder = false, bool isCharacter = false)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            clueButtonSetting,
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
        // button.SetBoard(boardRoots);
        // button.SetView(this);
        // button.SetProof(proof);
        // button.SetCharacter(isCharacter);
        button.MoveToLast();
        return button;
    }

    private void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }


    // public async UniTask TryToSolveCase(TextMeshProUGUI solveText)
    // {
    //     // foreach(var item in boardRoots) 
    //     // { 
    //     //     var choice = item.Value.droppedClue;
    //     //     //var rightChoice = manager.correctAnswer.FirstOrDefault(x => x.Key == item.Key);
    //     //
    //     //     //print(choice.GetProof());
    //     //     if (choice != null && choice.GetProof().Contains(item.Key)) continue;
    //     //     else
    //     //     {
    //     //         // manager.ConsumeAttempt();
    //     //         // if (manager.attemptsLeft > 0) await ShowError(solveText);
    //     //         // else manager.FailCase();
    //     //         // return;
    //     //     }
    //     // }
    //     //
    //     // manager.SolveCase();
    // }

    // public async UniTask ShowError(TextMeshProUGUI solveText)
    // {
    //     var oldText = solveText.text;
    //
    //     print("unsolved");
    //     //solveText.text = "Not quite";
    //     // solveText.text = $"Wait... this doesn't make enough sense. " +
    //     //                  $"\n I need to find a better theory, quickly... " +
    //     //                  $"\n [{manager.attemptsLeft} attempts left]";
    //     await DisplayErrorUI();
    //     solveText.text = oldText;
    // }

    // public async UniTask DisplayErrorUI()
    // {
    //     solveCanvas.transform.parent.gameObject.SetActive(true);
    //     while (solveCanvas.color.a < 0.8f)
    //     {
    //         solveCanvas.color += new Color(0, 0, 0, 0.06f);
    //         solveText.color += new Color(0, 0, 0, 0.06f);
    //         //shadeUI.color += new Color(0, 0, 0, 0.02f * timeToFadeUI / 5);
    //         await UniTask.Delay(20);
    //     }
    //
    //     await UniTask.Delay(2000);
    //
    //     while (solveCanvas.color.a > 0)
    //     {
    //         solveCanvas.color -= new Color(0, 0, 0, 0.03f);
    //         solveText.color -= new Color(0, 0, 0, 0.03f);
    //         //shadeUI.color -= new Color(0, 0, 0, 0.015f * timeToFadeUI / 5);
    //         await UniTask.Delay(20);
    //     }
    //     solveCanvas.transform.parent.gameObject.SetActive(false);
    //
    // }
    //

}

