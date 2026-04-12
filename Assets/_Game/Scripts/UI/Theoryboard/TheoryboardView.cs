using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using TMPro;
using System;

public class TheoryboardView : MonoBehaviour
{
    [SerializeField] NotebookManager notebookManager;
    [SerializeField] TheoryboardManager manager;
    public Transform markedLogsRoot;
    public Transform markedItemsRoot;
    public Transform boardRoot;
    public ButtonSetting clueButtonSetting;
    public Button solveButton;
    public TextMeshProUGUI solveText;

    public SerializedDictionary<TheoryboardManager.Whodunnit, TheoryPanel> boardRoots = new();

    public void LoadMarkedClues()
    {
        if (notebookManager.markedClues.Count <= 0) return;

        var markedLogs = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Log);
        print("Marked logs: " + markedLogs.Count());
        var markedItems = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Objects);
        print("Marked items: " + markedItems.Count());

        foreach (var log in markedLogs) 
        {
            var button = CreateClueButton(log.Value.displayName, markedLogsRoot, log.Value.isProof);
            button.AddListener(() =>
            {

            });
        }
        foreach (var item in markedItems)
        {
            var button = CreateClueButton(item.Value.displayName, markedItemsRoot, item.Value.isProof);
        }

    }

    public ButtonFactoryObject CreateClueButton(string text, Transform parent, List<TheoryboardManager.Whodunnit> proof)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            clueButtonSetting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );

        button.SetText(text);
        button.SetInteractable(true);
        button.SetBoard(boardRoots);
        button.SetView(this);

        button.SetProof(proof);

        return button;
    }

    public async UniTask TryToSolveCase(TextMeshProUGUI solveText)
    {
        foreach(var item in boardRoots) 
        { 
            var choice = item.Value.droppedClue;
            //var rightChoice = manager.correctAnswer.FirstOrDefault(x => x.Key == item.Key);

            if (choice != null && choice.GetProof().Contains(item.Key)) continue; else
            {
                print("unsolved"); await ShowError(solveText); return;
            }
        }
        
        manager.SolveCase();
    }

    public async UniTask ShowError(TextMeshProUGUI solveText)
    {
        var oldText = solveText.text;

        solveText.text = "Not quite";
        await UniTask.Delay(700);
        solveText.text = oldText;
    }

    void Start()
    {
        solveButton.onClick.AddListener(() => TryToSolveCase(solveText));
        //print(boardRoots.Count);
    }

}
