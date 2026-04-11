using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class TheoryboardView : MonoBehaviour
{
    [SerializeField] NotebookManager notebookManager;
    [SerializeField] TheoryboardManager manager;
    public Transform markedLogsRoot;
    public Transform markedItemsRoot;
    public Transform boardRoot;
    public ButtonSetting clueButtonSetting;

    [ShowInInspector, TableList] public Dictionary<TheoryboardManager.Whodunnit, Transform> boardRoots;

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

    public ButtonFactoryObject CreateClueButton(string text, Transform parent, TheoryboardManager.Whodunnit proof)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            clueButtonSetting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );

        button.SetText(text);
        button.SetInteractable(true);
        button.SetBoard(boardRoot);
        button.SetView(this);
        
        button.SetProof(proof);

        return button;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
