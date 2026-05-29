using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Unity.VisualScripting;
using System;

public class MarkingPanelView : MonoBehaviour, IActivity
{
    [SerializeField] IActivityEvent pushEvent;
    [SerializeField] EventChannel popEvent;
    [SerializeField] BoolEventChannel enableCursor;

    [SerializeField] GameObject mainUI;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI tipText;
    [SerializeField] Button markClueButton, cancelButton;
    
    
    [SerializeField] private NoteEvent m_markNoteEvent;
    private Dictionary<PageType, List<string>> _tips = new()
    {
        { PageType.Character, new List<string>()
        {
            "e.g. 'X claims Y hates Z.'",
            "e.g. 'X heard Y talking with Z.'",
            "e.g. 'X thinks Y is hiding something.'",
            "e.g. 'X said they know about Y.'",
            "e.g. 'X avoided talking about Y.'",
            "e.g. 'X saw Y carrying Z.'",
        }},
        { PageType.Objects, new List<string>()
        {
            "e.g. 'X was used by Y.'",
            "e.g. 'X thinks this is dangerous.'",
            "e.g. 'X knows about this Y.'",
            "e.g. 'X might belong to Y.'",
            "e.g. 'X was given to Y by Z.'",
            "e.g. 'X gonna give it to Y.'",
        }},
    };

    [HideInInspector] public Dictionary<SerializableGuid, ButtonFactoryObject> markableClues = new();

    private void OnDestroy()
    {
        _newClueName = default;
        isMarkingClue = false;
    }

    private void Update()
    {
        if (_currentClue != null)
        {
            if (Input.GetKeyDown(KeyCode.Return)) OnMarking?.Invoke(_currentClue);
            else if (Input.GetKeyDown(KeyCode.Space)) inputField.ActivateInputField();

        }
            
    }

    string _newClueName = default;
    [HideInInspector] public bool isMarkingClue = false;
    public event Action<Note> OnMarking = delegate { };
    Note _currentClue = null;
    public async UniTask RenameAndMarkClue(Note clue)
    {
        _newClueName = "";
        pushEvent.Raise(this);
        enableCursor.Raise(true);
        tipText.text = RandomTip(clue.type);
        await UnfoldPanel(true);
        _currentClue = clue;
        OnMarking += Mark;
        markClueButton.onClick.AddListener(async () =>
        {
            OnMarking?.Invoke(clue);
        });

        cancelButton.onClick.AddListener(async () =>
        {
            _newClueName = default;
            _currentClue = null;
            OnMarking -= Mark;
            AudioManager.Instance.SelectSFX(SFXType.Player, "FlipBackwards");
            await UnfoldPanel(false);
            popEvent.Raise();
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
    private void Mark(Note clue) => _ = Marking(clue);
    private async UniTask Marking(Note clue)
    {
        _currentClue = null;
        Note newClue = new Note(clue.displayName, clue.isProof)
        {
            type = clue.type
        };
        newClue.displayName = _newClueName != "" ? _newClueName.FirstCharacterToUpper() : newClue.displayName;

        // if (!NotebookManager.Instance.MarkedClues.Remove(clue.guid)) NotebookManager.Instance.MarkedClues.TryAdd(clue.guid, newClue);

        _newClueName = null;
     

        AudioManager.Instance.SelectSFX(SFXType.Player, "Scribble");
        inputField.text = "Sending to the Theory Board...";
        markClueButton.interactable = false; cancelButton.interactable = false; inputField.interactable = false;
        await UniTask.Delay(500);

        popEvent.Raise();
        await UnfoldPanel(false);
        Destroy(gameObject);
    }

    private async UniTask UnfoldPanel(bool isOpening)
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

    private string RandomTip(PageType type) => _tips[type][UnityEngine.Random.Range(0, _tips[type].Count - 1)];

    
    #region IActivity

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

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
    
    #endregion
}