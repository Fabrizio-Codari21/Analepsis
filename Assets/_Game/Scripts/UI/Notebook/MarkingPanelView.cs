using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Unity.VisualScripting;

public class MarkingPanelView : MonoBehaviour, IActivity
{
    [SerializeField] IActivityEvent pushEvent;
    [SerializeField] EventChannel popEvent;
    [SerializeField] BoolEventChannel enableCursor;

    [SerializeField] GameObject mainUI;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI tipText;
    [SerializeField] Button markClueButton, cancelButton;

    private Dictionary<NoteType, List<string>> _tips = new()
    {
        { NoteType.Log, new List<string>()
        {
            "e.g. 'X claims Y hates Z.'",
            "e.g. 'X heard Y talking with Z.'",
            "e.g. 'X thinks Y is hiding something.'",
            "e.g. 'X said they know about Y.'",
            "e.g. 'X avoided talking about Y.'",
            "e.g. 'X saw Y carrying Z.'",
        }},
        { NoteType.Objects, new List<string>()
        {
            "e.g. 'X was used by Y.'",
            "e.g. 'X thinks this is dangerous.'",
            "e.g. 'X knows about this Y.'",
            "e.g. 'X might belong to Y.'",
            "e.g. 'X was given to Y by Z.'",
            "e.g. 'X gonna give it to Y.'",
        }},
    };

    //[HideInInspector] public Dictionary<SerializableGuid, ButtonFactoryObject> markableClues = new();

    private void OnDestroy()
    {
        _newClueName = default;
        isMarkingClue = false;
    }

    string _newClueName = default;
    [HideInInspector] public bool isMarkingClue = false;
    public async UniTask RenameAndMarkClue(Note clue)
    {
        _newClueName = default;
        //if (NotebookManager.Instance.markedClues.ContainsKey(clue.guid))
        //{
        //    //markableClues[clue.guid]?.DisplayMark(false);
        //    return;
        //}

        ////markableClues[clue.guid]?.DisplayMark(true);
        pushEvent.Raise(this);
        enableCursor.Raise(true);
        tipText.text = RandomTip(clue.type);
        await UnfoldPanel(true);

        markClueButton.onClick.AddListener(async () =>
        {
            Note newClue = clue.type == NoteType.Log
            ? new LogNote(clue.displayName, "", clue.isProof)
            : new ItemNote(clue.displayName, null, clue.isProof);

            newClue.displayName = _newClueName != default ? _newClueName : newClue.displayName;

            if (!NotebookManager.Instance.markedClues.Remove(clue.guid))
                NotebookManager.Instance.markedClues.TryAdd(clue.guid, newClue);
            
            _newClueName = default;
            print("Marked clue: " + newClue.displayName);

            inputField.text = "Sending to the Theory Board...";
            markClueButton.interactable = false; cancelButton.interactable = false; inputField.interactable = false;
            await UniTask.Delay(500);

            popEvent.Raise();
            await UnfoldPanel(false);
            NotebookManager.Instance.EnableButtons(true);
            NotebookManager.Instance.ResetMarkingPanel();
            Destroy(gameObject);
        });

        cancelButton.onClick.AddListener(async () =>
        {
            _newClueName = default;
            NotebookManager.Instance.EnableMark(false);
            await UnfoldPanel(false);
            popEvent.Raise();
            NotebookManager.Instance.EnableButtons(true);
            NotebookManager.Instance.ResetMarkingPanel();
            Destroy(gameObject);
        });

        inputField.onEndEdit.AddListener(newName =>
        {
            _newClueName = newName;
        });
        inputField.onValueChanged.AddListener(newValue =>
        {
            if (newValue == "") tipText.text = RandomTip(clue.type);
        });
    }

    public async UniTask UnfoldPanel(bool isOpening)
    {
        if (mainUI == null) return;
        Tween.StopAll(mainUI.gameObject.transform);

        var seq = PrimeTween.Sequence.Create();

        if (isOpening)
        {
            mainUI.gameObject.transform.localScale = new Vector3(0, 1, 1);
            await seq.Group(Tween.ScaleX(mainUI.gameObject.transform, 1f, 0.3f, Ease.OutBack));
        }
        else
        {
            await seq.Group(Tween.ScaleX(mainUI.gameObject.transform, 0f, 0.2f, Ease.InQuad));
        }


        await seq;

    }

    public string RandomTip(NoteType type) => _tips[type][Random.Range(0, _tips[type].Count - 1)];


    public event System.Action OnResume;
    public event System.Action OnPause;
    public event System.Action OnStop;

    public void Resume()
    {
        OnResume?.Invoke();
    }

    public void Pause()
    {
        OnPause?.Invoke();
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }

    public bool CanPopWithKey()
    {
        return true;
    }
}
