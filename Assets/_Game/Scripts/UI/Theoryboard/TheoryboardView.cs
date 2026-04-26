using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using TMPro;
public class TheoryboardView : MonoBehaviour
{
    [SerializeField] NotebookManager notebookManager;
    [SerializeField] TheoryboardManager manager;
    public Transform markedLogsRoot;
    public Transform markedItemsRoot;
    public Transform markedCharactersRoot;
    public ButtonSetting clueButtonSetting;
    public Button solveButton;
    public TextMeshProUGUI solveText;

    public SerializedDictionary<Whodunnit, TheoryPanel> boardRoots = new();


    private IActivity _activity;
    
    void Start()
    {
        solveButton.onClick.AddListener(() => _ = TryToSolveCase(solveText));
        
        _activity = manager.GetComponent<IActivity>();

        _activity.OnStop += () =>
        {
            Despawn(markedLogsRoot);
            Despawn(markedItemsRoot);
        };
    }
    
    

    public void LoadMarkedClues()
    {
        var markedLogs = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Log);
        if(markedLogs.Count() <= 0)
            CreateClueButton("No Logs marked \n(Click the star to mark)", markedLogsRoot, default, true);
        var markedItems = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Objects);
        if (markedItems.Count() <= 0)
            CreateClueButton("No Objects marked \n(Click the star to mark)", markedItemsRoot, default, true);

        foreach(var character in NotebookManager.Instance.FoundCharacters)
        {
            //placeholder, despues hago que sea mas adecuado y no solo los personajes que tienen pistas descubiertas.
            var button = CreateClueButton(character.Key.npcName, markedCharactersRoot, new() { character.Key.role }, isCharacter: true);
        }

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
