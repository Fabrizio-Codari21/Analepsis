using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Linq;
using Unity.VisualScripting;

public class NotebookManager : Singleton<NotebookManager>, IActivity
{
    
    #region  Inputs & Cursor
    
    [SerializeField] private EventChannel m_openNotebookChannel;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private NoteBookInputReader inputReaderNoteBook;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
 
    #endregion
    [SerializeField] private EventChannel takeOutNotebookChannel;
    [SerializeField] private EventChannel putInNotebookChannel;
    [SerializeField] private NotebookView m_view;
    [SerializeField] private RecordNoteEvent m_recordNote;
    [SerializeField] private MarkingPanelView m_markingPanel;
    [SerializeField] private BoolEventChannel m_udpatePoi;
    private CancellationTokenSource _cts;
    [ReadOnly,ShowInInspector] private NoteType _currentNoteType;
    [ReadOnly, ShowInInspector] public Dictionary<SerializableGuid, Note> markedClues = new();
    private readonly Dictionary<SerializableGuid, HashSet<string>> _unlockedPoisByItem = new(); // punto de interes
    private readonly Dictionary<SerializableGuid,Note> _notebookPages = new();
    private readonly Dictionary<Item, string> _unlockedFlashbackNote = new();
    private Dictionary<NpcIdentity, List<LogNote>> _characterLogs = new();
    public Dictionary<NpcIdentity, List<LogNote>> FoundCharacters => _characterLogs;
    [SerializeField] MarkClueEvent markedClueEvent;

    public bool HasAllPois(Item item)
    {
        if (!_unlockedPoisByItem.TryGetValue(item.guid, out var unlockedIds)) return false;

        if (item.pois == null || item.pois.Count == 0) return true;

        foreach (var poi in item.pois)
        {
            if (!unlockedIds.Contains(poi.poiId)) return false;
        }

        return true;
    }

    public void AddCharacter(NpcIdentity npc)
    {
        if (!_characterLogs.ContainsKey(npc)) _characterLogs.Add(npc, new List<LogNote>());
    }
    public void AddLogToCharacter(NpcIdentity chara, LogNote log)
    {
        foreach (var character in _characterLogs)
        {
            if (character.Value.Contains(ReturnIfUnique(log, chara))) return;
        }

        if (_characterLogs.ContainsKey(chara)) _characterLogs[chara].Add(log);
        else _characterLogs.Add(chara,new(){log});
    }

    // Devuelve la nota original si es unica o la ya existente si su informacion es igual.
    public Note ReturnIfUnique(Note note, NpcIdentity character = default)
    {
        List<Note> otherNotes = note.type == NoteType.Log
        ? (_characterLogs.ContainsKey(character) ? new(_characterLogs[character]) : new())
        : _notebookPages.Values.ToList();

        foreach(Note existingNote in otherNotes)
        {
            if(note.GetInfo() == existingNote.GetInfo()) return existingNote;
        }
        return note;
    }
   
    private void Start()
    {
        m_view = Instantiate(m_view,transform);
        inputReaderNoteBook.Close += Close;
        m_recordNote.OnEventRaised += Record;
        ResetMarkingPanel();
        markedClueEvent.OnEventRaised += async (note) => await TryToMarkClue(note);
        m_openNotebookChannel.OnEventRaised += Open;
    }

    private void Record(Note note)
    {
        if (!_notebookPages.TryAdd(note.guid, note))
        {
           
        }
  
        //MarkClue(note); //esto es temporal
    }

    private async UniTask TryToMarkClue(Note note)
    {
        var panel = Instantiate(m_markingPanel,transform);
        await panel.RenameAndMarkClue(note);
    }
    public void ResetMarkingPanel() => m_markingPanel.isMarkingClue = false;
    public event Action<bool> enableButtonsEvent = delegate { };
    public event Action<bool> enableMarkEvent = delegate { };
    public event Action<bool> closeAllButtonsEvent = delegate { };
    public void EnableButtons(bool enable) => enableButtonsEvent?.Invoke(enable);
    public void EnableMark(bool enable) => enableMarkEvent?.Invoke(enable);

    private void Open()
    {
        pushEvent.Raise(this);
        _ = ActionTimer.Instance.m_view.DisplayUI();       
    }

    private void Close()
    {
        popEvent.Raise();
        closeAllButtonsEvent?.Invoke(false); closeAllButtonsEvent = default;
    }
    
    public void UnlockPoi(Item item, string poiId)
    {
        if (!_unlockedPoisByItem.ContainsKey(item.guid))
        {
            _unlockedPoisByItem[item.guid] = new HashSet<string>();
        }
        
       
        var set = _unlockedPoisByItem[item.guid];
        
        bool wasCompleteBefore = HasAllPois(item);
        if (!set.Add(poiId)) return;
        
        var poiData = item.pois.Find(x => x.poiId == poiId);
       
        bool isCompleteNow = HasAllPois(item);
        if (wasCompleteBefore != isCompleteNow)
        {
            m_udpatePoi.Raise(isCompleteNow);
        }
    }
    
    public List<string> GetUnlockedPoiDescriptions(Item item)
    {
        List<string> descriptions = new();
        if (!_unlockedPoisByItem.TryGetValue(item.guid, out var unlockedIds)) return descriptions;
        foreach (var poiData in item.pois)
        {
            if (unlockedIds.Contains(poiData.poiId))
            {
                descriptions.Add(poiData.description);
            }
        }
        return descriptions;
    }



    public void UpdateFlashbackInfo(Item item, string info)
    {
        if (!_unlockedFlashbackNote.TryAdd(item, info))return;
    }

    public string GetItemFlashbackInfo(Item item)
    {
        return !_unlockedFlashbackNote.TryGetValue(item, out var flashback) ? string.Empty : flashback;
    }

    private void OpenNotebookByType(NoteType type)
    {
        _currentNoteType = type;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        
        m_view.ClearDetail();
        m_view.ClearButton();
        enableButtonsEvent = default; enableMarkEvent = default;
        //m_markingPanel.markableClues.Clear();
        m_view.SetTitle(type.ToString()); // si vamos a hacer localization ya deberia usar de esta forma , lo hago asi para ahorrarme tiempo
        // no es muy solid que digamos pero para probar por ahora sirve
        if(type == NoteType.Log && _characterLogs.Count > 0)
        {
            foreach(var character in _characterLogs)
            {
                var charButton = m_view.CreateButton(character.Key.npcName + " Logs");
                charButton.gameObject.transform.localScale *= 1.1f;
                charButton.DisableSub();
                closeAllButtonsEvent += charButton.MakeOpen;

                charButton.AddListener(() =>
                {
                    if (!charButton.IsOpen())
                    {
                        if (character.Value.Count > 0)
                        {
                            foreach (var item in character.Value)
                            {
                                var button = SpawnClueButton(item);
                                button.EnableSub();
                                button.MoveToPosition(charButton.GetPosition() + (character.Value.IndexOf(item) + 1));
                                button.gameObject.transform.localScale *= 0.9f;
                                charButton.AddToChildren(button);
                                item.parentButton = button;
                            }
                            charButton.MakeOpen(true);
                        }
                        else
                        {
                            var button = m_view.CreateButton($"No saved conversations.");
                            button.DisableSub();
                            button.SetInteractable(false);
                            button.MoveToPosition(charButton.GetPosition() + 1);
                            button.gameObject.transform.localScale *= 0.9f;
                            charButton.AddToChildren(button);
                            charButton.MakeOpen(true);
                        }
                    }
                    else
                    {
                        charButton.ClearChildren();
                        charButton.MakeOpen(false);
                        m_view.ClearDetail();
                    }
                });
            }
        }
        else
        {
            if (_notebookPages.Where(x => x.Value.type == type).Count() <= 0)
            {
                var button = m_view.CreateButton($"No {type} found yet.");
                button.EnableSub(false); button.SetInteractable(false); return;
            }
            foreach (var note in _notebookPages.Values)
            {
                if (note.type != type) continue;
                var cachedNote = note;
                var button = SpawnClueButton(cachedNote);
                button.EnableSub();
                button.gameObject.transform.localScale *= 1.1f;
            }
        }

        //print("markable clues: " + m_markingPanel.markableClues.Count);
    }

    public ButtonFactoryObject SpawnClueButton(Note cachedNote)
    {
        var button = m_view.CreateButton(cachedNote.GetButtonText());
        //m_markingPanel.markableClues.Add(cachedNote.guid, button);
        button.AddListener(() =>
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            m_view.ClearDetail();
            _ = SelectNote(button, cachedNote, _cts.Token);
        });
        //button.MoveSubToLast();
        button.EnableSub();
        enableButtonsEvent += button.EnableSub;
        button.AddListenerToSub(() =>
        {
            //_cts?.Cancel();
            //_cts?.Dispose();
            //_cts = new CancellationTokenSource();
            if (markedClues.ContainsKey(cachedNote.guid))
            {
                button.DisplayMark(false);
                markedClues.Remove(cachedNote.guid);
                return;
            }
            m_markingPanel.isMarkingClue = true;

            button.DisplayMark(true);
            enableMarkEvent = button.DisplayMark;
            EnableButtons(false);
            markedClueEvent.Raise(cachedNote);
        });

        return button;
    }

    private async UniTask SelectNote(ButtonFactoryObject parent, Note note, CancellationToken token)
    {
        try
        {
            m_view.ClearDetail();
            await note.Show(m_view, token);
            token.ThrowIfCancellationRequested();
            AddDetailButtons(parent, m_view, note);

        }
        catch (OperationCanceledException)
        {
        }
    }

    public void AddDetailButtons(ButtonFactoryObject parent, NotebookView view, Note note)
    {
        var clearButton = view.CreateDetailButton("Clear");
        clearButton.AddListener(() =>
        {
            view.ClearDetail();
        });
        var deleteButton = m_view.CreateDetailButton("Delete Log");
        deleteButton.AddListener(() =>
        {
            m_view.ClearDetail();
            if (note.type == NoteType.Log)
            {
                foreach (var character in _characterLogs)
                {
                    if (character.Value.Contains(note))
                    {
                        character.Value.Remove((LogNote)note);
                    }
                }

                var button = m_view.CreateButton($"No saved conversations.");
                button.transform.parent = parent.transform.parent;
                button.DisableSub();
                button.SetInteractable(false);
                button.MoveToPosition(parent.GetPosition());
                button.gameObject.transform.localScale *= 0.9f;
                parent.GetParent().AddToChildren(button);
            }
            else _notebookPages.Remove(note.guid);

            parent.RemoveFromParent();
            Destroy(parent.gameObject);

        });
    }

    private void ChangeType(float direction)
    {
        if(direction == 0 ) return;
        var values = (NoteType[])Enum.GetValues(typeof(NoteType));
        var currentIndex = (int)_currentNoteType;
        
        currentIndex += direction > 0 ? 1 : -1;
        
        if (currentIndex >= values.Length) currentIndex = 0;
        else if (currentIndex < 0) currentIndex = values.Length - 1;
        
        OpenNotebookByType(values[currentIndex]);
    }
    
    #region Interface
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderNoteBook.SetEnable();
        if (m_markingPanel.isMarkingClue) return;

        m_view.gameObject.SetActive(true);
        m_view.NextButtonAdd(()=> ChangeType(1));
        m_view.PreviousButtonAdd(()=>ChangeType(-1));
        inputReaderNoteBook.Flip += ChangeType;
        takeOutNotebookChannel.Raise();
        enableCursor.Raise(true);
        OpenNotebookByType(_currentNoteType);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        inputReaderNoteBook.SetEnable(false);
        if (m_markingPanel.isMarkingClue) return;

        m_view.gameObject.SetActive(false);
        m_view.RemoveNext();
        m_view.RemovePrevious();
        enableCursor.Raise(false);
          putInNotebookChannel.Raise();
        inputReaderNoteBook.Flip -= ChangeType;
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
     
    public bool CheckNote(SerializableGuid guid) => _notebookPages.ContainsKey(guid);
    
}



public enum NoteType
{
    Log,
    Objects,
}
public enum Emotion
{
    None,
    Idle,
    Worried,
    Angry,
    Happy,
    Sad
}

[Serializable]
public  class Note
{
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public NoteType type;
    public string displayName;
    public List<Whodunnit> isProof;

    public Note(string displayName,  List<Whodunnit> proof = null)
    {
        this.displayName = displayName;
        this.isProof = proof;


    }

    public virtual string GetButtonText()
    {
        return displayName;
    }
    public virtual UniTask Show(NotebookView view, CancellationToken token)
    {
        return UniTask.CompletedTask;
    }
    public virtual string GetInfo() => default;
}




public class LogNote : Note
{
 
    private string _fullInfo;   
    private string _recordInfo;
    private bool _showingFull  = false;
    public ButtonFactoryObject parentButton;
    public LogNote(string displayName,string fullInfo, string recordInfo,List<Whodunnit> proof = null) : base(displayName, proof)
    {
        _fullInfo = fullInfo;
        _recordInfo = recordInfo;
        type = NoteType.Log;
    }
    public void UpdateLog(LogNote log)
    {
        // aca agregamos todo lo que queramos actualizar cuando la conversacion
        // se repite (si marcaste cosas distintas, etc).
        _recordInfo = log._recordInfo;
    }
   public override async UniTask Show(NotebookView view, CancellationToken token)
    {
        _showingFull = false; 
        await RefreshDisplay(view, token, true);
    }

    private async UniTask RefreshDisplay(NotebookView view, CancellationToken token, bool firstTime = false)
    {
        view.ClearDetail(); 
        string contentToShow = _showingFull ? _fullInfo : 
            (_recordInfo == "" 
            ? "[No highlighted text (Click on a piece of dialogue while talking to someone to highlight it.)]\n\n"
            : _recordInfo);

        string header = _showingFull ? "<b>[FULL TRANSCRIPT]</b>\n\n" : "<b>[HIGHLIGHTS]</b>\n\n";
        await view.PlayText(new() { header + contentToShow }, token);
        if (token.IsCancellationRequested) return;
        
        string buttonLabel = _showingFull ? "See Highlights" : "See Full Transcript";
        var toggleBtn = view.CreateDetailButton(buttonLabel);
        toggleBtn.AddListener(() =>
        {
            _showingFull = !_showingFull;
            _ = RefreshDisplay(view, token, false);
        });
        if(!firstTime) NotebookManager.Instance.AddDetailButtons(parentButton, view, this);

    }
    public override string GetInfo() => _fullInfo;
}

public class ItemNote : Note
{
    private readonly Item _item;
    public ItemNote(string displayName,Item item, List<Whodunnit> proof = null) : base(displayName, proof)
    {
        type = NoteType.Objects;
        if (item == null) return;
        _item =  item;
        guid = _item.guid;  // para usa el guid de item para que solamente anota el item y no se repite, pero el clue se puede ir desbloqueando de a poco
    }

    public List<string> FullInfo()
    {
        List<string> fullContent = new List<string>();
        var unlockedDescriptions = NotebookManager.Instance.GetUnlockedPoiDescriptions(_item);

        foreach (var desc in unlockedDescriptions)
        {
            fullContent.Add($"{unlockedDescriptions.IndexOf(desc) + 1})  {desc}");
        }

        var unlockedFlash = NotebookManager.Instance.GetItemFlashbackInfo(_item);
        if (unlockedFlash != string.Empty) fullContent.Add($"FLASHBACK :  {unlockedFlash}");
        return fullContent;
    }

    public override async UniTask Show(NotebookView view, CancellationToken token)
    {
        view.CreateImage(_item.sprite);
    
        
        await view.PlayText(FullInfo(), token);
    }
    public override string GetInfo()
    {
        string str = string.Empty;
        var info = FullInfo();
        foreach (var desc in info) str += desc;
        return str;
    }
}
