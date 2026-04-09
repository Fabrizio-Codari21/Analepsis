using UnityEngine;
using System.Linq;

public class TheoryboardView : MonoBehaviour
{
    [SerializeField] NotebookManager notebookManager;
    [SerializeField] TheoryboardManager manager;
    public Transform markedLogsRoot;
    public Transform markedItemsRoot;
    public ButtonSetting clueButtonSetting;

    public void LoadMarkedClues()
    {
        if (notebookManager.markedClues.Count <= 0) return;

        var markedLogs = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Log);
        print("Marked logs: " + markedLogs.Count());
        var markedItems = notebookManager.markedClues.Where(x => x.Value.type == NoteType.Objects);
        print("Marked items: " + markedItems.Count());

        foreach (var log in markedLogs) 
        {
            var button = CreateClueButton(log.Value.displayName, markedLogsRoot);
        }
        foreach (var item in markedItems)
        {
            var button = CreateClueButton(item.Value.displayName, markedItemsRoot);
        }

    }

    private ButtonFactoryObject CreateClueButton(string text, Transform parent)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            clueButtonSetting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );

        button.SetText(text);
        button.SetInteractable(true);

        return button;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
