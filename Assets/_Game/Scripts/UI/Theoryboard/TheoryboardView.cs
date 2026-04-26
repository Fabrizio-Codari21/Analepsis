using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.TextCore.Text;
public class TheoryboardView : MonoBehaviour
{
    [SerializeField] NotebookManager notebookManager;
    [SerializeField] TheoryboardManager manager;
    public Transform markedLogsRoot;
    public Transform markedItemsRoot;
    public Transform markedCharactersRoot;
    public Button previousCharacterButton, nextCharacterButton;
    public ButtonSetting clueButtonSetting;
    public Button solveButton;
    public TextMeshProUGUI solveText;

    public SerializedDictionary<Whodunnit, TheoryPanel> boardRoots = new();


    private IActivity _activity;
    
    void Start()
    {
        solveButton.onClick.AddListener(() => _ = TryToSolveCase(solveText));
        previousCharacterButton.onClick.AddListener(() => SwitchCharacter(-1));
        nextCharacterButton.onClick.AddListener(() => SwitchCharacter(1));
        
        _activity = manager.GetComponent<IActivity>();

        _activity.OnStop += () =>
        {
            Despawn(markedLogsRoot);
            Despawn(markedItemsRoot);
        };
    }


    int _currentCharacter = 0;
    public void LoadMarkedClues()
    {
        var markedLogs = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Log);
        if(markedLogs.Count() <= 0)
            CreateClueButton("No Logs marked \n(Click the star to mark)", markedLogsRoot, default, true);
        var markedItems = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Objects);
        if (markedItems.Count() <= 0)
            CreateClueButton("No Objects marked \n(Click the star to mark)", markedItemsRoot, default, true);

        SwitchCharacter();
        
        if (notebookManager.markedClues.Count <= 0) return;

        foreach (var log in markedLogs) 
        {
            var button = CreateClueButton(log.Value.displayName, markedLogsRoot, log.Value.isProof);
            
           
        }
        foreach (var item in markedItems)
        {
            var button = CreateClueButton(item.Value.displayName, markedItemsRoot, item.Value.isProof);
        }

    }

    public void SwitchCharacter(int nextOrPrevious = 0)
    {
        Despawn(markedCharactersRoot);
        var character = NotebookManager.Instance.FoundCharacters.Keys.ToList()[_currentCharacter];
        if (nextOrPrevious > 0)
        {
            _currentCharacter++;
            if (_currentCharacter > NotebookManager.Instance.FoundCharacters.Count - 1) _currentCharacter = 0;
        }
        else if (nextOrPrevious < 0)
        {
            _currentCharacter--;
            if (_currentCharacter < 0) _currentCharacter = NotebookManager.Instance.FoundCharacters.Count - 1;
        }
        var charButton = CreateClueButton(character.npcName, markedCharactersRoot, new() { character.role }, isCharacter: true);
    }

    public ButtonFactoryObject CreateClueButton(string text, Transform parent, List<Whodunnit> proof, bool placeholder = false, bool isCharacter = false)
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
        button.SetBoard(boardRoots);
        button.SetView(this);
        button.SetProof(proof);
        button.SetCharacter(isCharacter);
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


    public async UniTask TryToSolveCase(TextMeshProUGUI solveText)
    {
        foreach(var item in boardRoots) 
        { 
            var choice = item.Value.droppedClue;
            //var rightChoice = manager.correctAnswer.FirstOrDefault(x => x.Key == item.Key);

            if (choice != null && choice.GetProof().Contains(item.Key)) continue; else
            {
                await ShowError(solveText); return;
            }
        }
        
        manager.SolveCase();
    }

    public async UniTask ShowError(TextMeshProUGUI solveText)
    {
        var oldText = solveText.text;

        print("unsolved");
        solveText.text = "Not quite";
        await UniTask.Delay(700);
        solveText.text = oldText;
    }

  

}
