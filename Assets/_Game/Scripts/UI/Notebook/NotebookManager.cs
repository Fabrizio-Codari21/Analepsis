using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Linq;


public class NotebookManager : Singleton<NotebookManager>, IActivity
{
    
    #region  Inputs & Cursor
    
    [SerializeField] private EventChannel m_openNotebookChannel;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private NoteBookInputReader inputReaderNoteBook;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
 
    #endregion
    
    
    #region Event
    [Header("Takeout and Put Events")]
    [SerializeField] private TakeableEvent takeOutNotebookChannel; // cuando saca
    [SerializeField] private TakeableEvent putInNotebookChannel; // cuando guarda
    [Header("Notebook Core Events")]
    [SerializeField] private RecordNoteEvent m_recordNote; // record 
    [SerializeField] private BoolEventChannel m_udpatePoi;
    [SerializeField] public MarkClueEvent markedClueEvent;
    #endregion
    
    [SerializeField] private NotebookRepresenter representer;
    [SerializeField] public MarkingPanelView m_markingPanel;
    [ReadOnly,ShowInInspector] private PageType _currentPageType;
    [ReadOnly, ShowInInspector] public Dictionary<SerializableGuid, Note> MarkedClues = new();
    private readonly Dictionary<SerializableGuid, HashSet<string>> _unlockedPoisByItem = new(); // punto de interes
    private readonly Dictionary<SerializableGuid,Note> _notebookPages = new();
    private readonly Dictionary<Item, string> _unlockedFlashbackNote = new();
    private Dictionary<NpcIdentity, List<LogNote>> _characterLogs = new();

    private CancellationTokenSource _cts;
    
    public Dictionary<NpcIdentity, List<LogNote>> FoundCharacters => _characterLogs;
    public List<DialogueNote> StartedDialogues { get; } = new();


    #region Poi
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
    #endregion
    
    #region Character

    public Dictionary<NpcIdentity, List<LogNote>> CharacterLogs
    {
        get => _characterLogs;
        set => _characterLogs = value;
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
        List<Note> otherNotes = note.type == PageType.Character
        ? (_characterLogs.ContainsKey(character) ? new(_characterLogs[character]) : new())
        : _notebookPages.Values.ToList();

        foreach(Note existingNote in otherNotes)
        {
            if(note.GetInfo() == existingNote.GetInfo()) return existingNote;
        }
        return note;
    }
    
    #endregion



    #region  Unity Life
    
 
    private void Start()
    {
      
        representer = Instantiate(representer);
        inputReaderNoteBook.Close += Close;
        m_recordNote.OnEventRaised += Record;
        markedClueEvent.OnEventRaised +=  MarkClue;
        m_openNotebookChannel.OnEventRaised += Open;
        
    }

    private void OnDestroy()
    {
        inputReaderNoteBook.Close -= Close;
        m_recordNote.OnEventRaised -= Record;
        markedClueEvent.OnEventRaised -= MarkClue;
        m_openNotebookChannel.OnEventRaised -= Open;
    }

    #endregion

    
    
    
    private void Record(Note note)
    {
        if (!_notebookPages.TryAdd(note.guid, note))
        {
           
        }
  
    }


    private void MarkClue(Note note)
    {
        TryToMarkClue(note).Forget();
    }

    private async UniTask TryToMarkClue(Note note)
    {
        var panel = Instantiate(m_markingPanel,transform);
        await panel.RenameAndMarkClue(note);
    }
    
    #region Take & Put

    private void Open()
    {
        pushEvent.Raise(this);
        AudioManager.Instance.SelectSFX(SFXType.Player, "Open");

        takeOutNotebookChannel.Raise(representer);

        _ = ActionTimer.Instance.m_view.DisplayUI();       
    }

    private void Close()
    {
        popEvent.Raise();

        AudioManager.Instance.SelectSFX(SFXType.Player, "Close");
        putInNotebookChannel.Raise(representer);
 
    }
    
    
    #endregion

    #region  Internal

    private void OpenNotebookByType(PageType type)
    {
       
      

    }

   
    private void ChangeType(float direction)
    {
        if(direction == 0 ) return;
        var values = (PageType[])Enum.GetValues(typeof(PageType));
        var currentIndex = (int)_currentPageType;
        
        currentIndex += direction > 0 ? 1 : -1;
        
        if (currentIndex >= values.Length) currentIndex = 0;
        else if (currentIndex < 0) currentIndex = values.Length - 1;
        
        OpenNotebookByType(values[currentIndex]);
    }
    
    
    
    #endregion
    
    #region External
    public CancellationTokenSource Cancel() 
    { 
        _cts?.Cancel(); 
        _cts?.Dispose(); 
        _cts = new CancellationTokenSource(); 
        return _cts; 
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


   

    public bool CheckNote(SerializableGuid guid) => _notebookPages.ContainsKey(guid);

#endregion
    
    #region IActivity
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderNoteBook.SetEnable();
        if (m_markingPanel.isMarkingClue) return;
        inputReaderNoteBook.Flip += ChangeType;
        enableCursor.Raise(true);
        OpenNotebookByType(_currentPageType);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        inputReaderNoteBook.SetEnable(false);
        if (m_markingPanel.isMarkingClue) return;
        enableCursor.Raise(false);
        inputReaderNoteBook.Flip -= ChangeType;
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }

    public bool CanPopWithKey()
    {
      return false;
    }
  
    #endregion
     
    
}


public enum PageType
{
    Character,
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
public enum Reaction
{
    None,
    Idle,
    Gesticulate,
    AvoidGaze,
    Laugh,
    GetNervous,
    Think,
    Generic,
}

[Serializable]
public  class Note
{
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public PageType type;
    public string displayName;
    public Tuple<Clue,List<Whodunnit>> isProof;

    public Note(string displayName,  Tuple<Clue,List<Whodunnit>> proof = null)
    {
        this.displayName = displayName;
        this.isProof = proof;


    }

    public virtual string GetButtonText()
    {
        return displayName;
    }
    public virtual UniTask Show(NotebookRepresenter representer, CancellationToken token)
    {
        return UniTask.CompletedTask;
    }
    public virtual string GetInfo() => default;
}




public class LogNote : Note
{
 
    private List<string> _fullInfo;   
    private List<string> _recordInfo;
    private bool _showingFull  = false;
    public ButtonFactoryObject parentButton;
    public LogNote(string displayName,List<string> fullInfo, List<string> recordInfo,Tuple<Clue, List<Whodunnit>> proof = null) : base(displayName, proof)
    {
        _fullInfo = fullInfo;
        _recordInfo = recordInfo;
        type = PageType.Character;
    }
    public void UpdateLog(LogNote log)
    {
        // aca agregamos todo lo que queramos actualizar cuando la conversacion
        // se repite (si marcaste cosas distintas, etc).
        _recordInfo = log._recordInfo;
    }
   public override async UniTask Show(NotebookRepresenter representer, CancellationToken token)
    {
        _showingFull = false; 
        await RefreshDisplay(representer, token, true);
    }

    private async UniTask RefreshDisplay(NotebookRepresenter representer, CancellationToken token, bool firstTime = false)
    {
        // representer.ClearDetail(); 
        // List<string> contentToShow = new(_showingFull ? _fullInfo : 
        //     (_recordInfo.Count <= 0 
        //     ? new(){"\n[No highlighted text (Click on a piece of dialogue while talking to someone to highlight it.)]\n\n" }
        //     : _recordInfo));
        //
        // contentToShow.Insert(0, _showingFull ? "<b>[FULL TRANSCRIPT]</b>" : "<b>[HIGHLIGHTS]</b>");
        // await representer.PlayText(contentToShow, token);
        // if (token.IsCancellationRequested) return;
        //
        // string buttonLabel = _showingFull ? "See Highlights" : "See Full Transcript";
        // var toggleBtn = representer.CreateDetailButton(buttonLabel);
        // toggleBtn.AddListener(() =>
        // {
        //     _showingFull = !_showingFull;
        //     _ = RefreshDisplay(representer, token, false);
        // });
        // if(!firstTime) NotebookManager.Instance.AddDetailButtons(parentButton, representer, this);

    }
    public override string GetInfo() => _fullInfo.AsString();
    public void ChangeRecord(List<string> records) => _recordInfo = records;
}

public class ItemNote : Note
{
    private readonly Item _item;
    public ItemNote(string displayName,Item item, Tuple<Clue, List<Whodunnit>> proof = null) : base(displayName, proof)
    {
        type = PageType.Objects;
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

    public override async UniTask Show(NotebookRepresenter representer, CancellationToken token)
    {
        // representer.CreateImage(_item.sprite);
        //
        //
        // await representer.PlayText(FullInfo(), token);
    }
    public override string GetInfo() => FullInfo().AsString();
}

// En el sistema de arbol, esto eventualmente reemplazaria a LogNote.
public class DialogueNote : Note
{
    private Dialogue _fullDialogue;
    private List<INode> _unlockedDialogue;
    private List<string> _fullInfo;
    private List<string> _recordInfo;
    private bool _showingFull = false;
    public ButtonFactoryObject parentButton;
    public DialogueNote(string displayName, Dialogue fullDialogue, List<INode> unlockedDialogue, Tuple<Clue, List<Whodunnit>> proof = null) : base(displayName, proof)
    {
        _fullDialogue = fullDialogue;
        _unlockedDialogue = unlockedDialogue;
        type = PageType.Character;
    }
    public void UpdateLog(DialogueNote log)
    {
        // aca agregamos todo lo que queramos actualizar cuando la conversacion
        // se repite (si marcaste cosas distintas, etc).
        foreach (INode node in log._unlockedDialogue)
        {
            if(!_unlockedDialogue.Contains(node)) _unlockedDialogue.Add(node);
        }
    }
    public override async UniTask Show(NotebookRepresenter representer, CancellationToken token)
    {
        _showingFull = false;
        await RefreshDisplay(_recordInfo, representer, token, true);
    }

    public async UniTask RefreshDisplay(List<string> text, NotebookRepresenter representer, CancellationToken token, bool firstTime = false)
    {
        // representer.ClearDetail();
        // List<string> contentToShow = new(_showingFull ? _fullInfo :
        //     (_recordInfo.Count <= 0
        //     ? new() { "\n[No highlighted text (Click on a piece of dialogue while talking to someone to highlight it.)]\n\n" }
        //     : text));
        //
        // contentToShow.Insert(0, _showingFull ? "<b>[FULL TRANSCRIPT]</b>" : "<b>[HIGHLIGHTS]</b>");
        // await representer.PlayText(contentToShow, token);
        // if (token.IsCancellationRequested) return;
        //
        // string buttonLabel = _showingFull ? "See Highlights" : "See Full Transcript";
        // var toggleBtn = representer.CreateDetailButton(buttonLabel);
        // toggleBtn.AddListener(() =>
        // {
        //     _showingFull = !_showingFull;
        //     _ = RefreshDisplay(text, representer, token, false);
        // });
        // if (!firstTime) NotebookManager.Instance.AddDetailButtons(parentButton, representer, this);

    }
    public override string GetInfo() => _fullInfo.AsString();
    public Dialogue GetFullDialogue() => _fullDialogue;
    public List<INode> GetUnlockedDialogue() => _unlockedDialogue;

    public void ChangeRecord(List<string> records) => _recordInfo = records;
}
