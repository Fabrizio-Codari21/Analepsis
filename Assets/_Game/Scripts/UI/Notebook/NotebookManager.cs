using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Linq;
using Sirenix.Utilities.Editor;


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
    [SerializeField] private BoolEventChannel m_updatePoi;
    #endregion
    
    
    #region General
    [SerializeField] private NotebookRepresenter representer;
    [ReadOnly,ShowInInspector] private PageType _currentPageType;
    private readonly Dictionary<SerializableGuid,Note> _notebookPages = new();
    [ReadOnly, ShowInInspector] public Dictionary<SerializableGuid, Note> MarkedClues = new();
    #endregion
    
    
    #region Item Page
    private readonly Dictionary<SerializableGuid, HashSet<string>> _unlockedPoisByItem = new(); // punto de interes
    private readonly Dictionary<Item, string> _unlockedFlashbackNote = new();
    #endregion
    

    #region Character Page

    public Dictionary<NpcIdentity, List<LogNote>> FoundCharacters { get; private set; } = new();
    public List<DialogueNote> StartedDialogues { get; } = new();

    #endregion
    

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
        get => FoundCharacters;
        set => FoundCharacters = value;
    }

    public void AddCharacter(NpcIdentity npc)
    {
        if (!FoundCharacters.ContainsKey(npc)) FoundCharacters.Add(npc, new List<LogNote>());
    }
    public void AddLogToCharacter(NpcIdentity chara, LogNote log)
    {
        foreach (var character in FoundCharacters)
        {
            if (character.Value.Contains(ReturnIfUnique(log, chara))) return;
        }

        if (FoundCharacters.ContainsKey(chara)) FoundCharacters[chara].Add(log);
        else FoundCharacters.Add(chara,new(){log});
    }

    
    // Devuelve la nota original si es unica o la ya existente si su informacion es igual.
    public Note ReturnIfUnique(Note note, NpcIdentity character = default)
    {
        List<Note> otherNotes = note.type == PageType.Character
        ? (FoundCharacters.ContainsKey(character) ? new(FoundCharacters[character]) : new())
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
        m_openNotebookChannel.OnEventRaised += Open;
        
    }

    private void OnDestroy()
    {
        inputReaderNoteBook.Close -= Close;
        m_recordNote.OnEventRaised -= Record;
    
        m_openNotebookChannel.OnEventRaised -= Open;
    }

    #endregion
    
    
    private void Record(Note note)
    {
        if (!_notebookPages.TryAdd(note.guid, note))
        {
           
        }
  
    }

    
    #region Open & Close
    private void Open()
    {
        pushEvent.Raise(this);
        AudioManager.Instance.SelectSFX(SFXType.Player, "Open");
        takeOutNotebookChannel.Raise(representer);
        
    }

    private void Close()
    {
        popEvent.Raise();

        AudioManager.Instance.SelectSFX(SFXType.Player, "Close");
        putInNotebookChannel.Raise(representer);
 
    }
    
    #endregion

    #region  Internal


    private void ChangeType(float direction)
    {
        switch (direction)
        {
            case 0:
                return;
            case > 0:
                representer.NextPage();
                return;
            case < 0:
                representer.PreviousPage();
                break;
        }
    }
    
    
    
    #endregion
    
    #region External

    #region  Item

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
            m_updatePoi.Raise(isCompleteNow);
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

    #endregion
    
    #region IActivity
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderNoteBook.SetEnable();
        inputReaderNoteBook.Flip += ChangeType;
        enableCursor.Raise(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        inputReaderNoteBook.SetEnable(false);
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
    
    
    
    // private void MarkClue(Note note)
    // {
    //     TryToMarkClue(note).Forget();
    // }
    //
    // private async UniTask TryToMarkClue(Note note)
    // {
    //     var panel = Instantiate(m_markingPanel,transform);
    //     await panel.RenameAndMarkClue(note);
    // }
    
}
[Serializable]
public abstract class NotebookLayout
{

    public int Index;

    public abstract void Initialize(Transform leftRoot, Transform rightRoot);

    public virtual void Hide()
    {
        
    }

    public virtual void Show()
    {
        
    }
}


public abstract class NotebookPage : MonoBehaviour
{
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }
}
[Serializable]
public class CharacterLayout :  NotebookLayout
{
    public CharacterNotebookPage characterNotebookPage;
    public TreePage treePage;
    public override void Initialize(Transform leftRoot, Transform rightRoot)
    {
        throw new NotImplementedException();
    }
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
