using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Linq;


public class NotebookManager : Singleton<NotebookManager>, IActivity,IMark
{
    
    #region  Inputs & Cursor
    
    [Header("Event")]
    [Header("Open Notebook")]
    [SerializeField] private EventChannel m_openNotebookChannel;
    
    [Header("Cursor")]
    [SerializeField] private BoolEventChannel enableCursor;
    [Header("Input")]
    [SerializeField] private NoteBookInputReader inputReaderNoteBook;
    [Header("Screen")]
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
    #endregion
    
    #region Events
    [Header("Hand Event")]
    [SerializeField] private EventChannel takeOutNotebookChannel;
    [SerializeField] private EventChannel putInNotebookChannel;
    [Header("Note ")]
    [SerializeField] private RecordNoteEvent m_recordNote;
    [Header("Poi")]
    [SerializeField] private BoolEventChannel m_udpatePoi;
    
    [Header("Mark Event")]
    [SerializeField,InfoBox("Sen to Theory board")] private MarkClueEvent markedClueEvent;
    #endregion
    
    [Header("Setting")]
    [SerializeField] private NotebookView m_view;
    [SerializeField] private MarkingPanel m_markingPanel;
    
    [ReadOnly,ShowInInspector] private NoteType _currentNoteType;
 
    #region Data
    #region Clue Mark
    private readonly Dictionary<SerializableGuid, Note> _markedClues = new();

    private bool _isMarking = false;
    public async void MarkClue(SerializableGuid guid)
    {
        if (_isMarking || IsMarked(guid)) return;
        if (!_allNotes.TryGetValue(guid, out var note)) return;
        
        _isMarking = true;
        inputReaderNoteBook.SetEnable(false);
        _virtualMouse.enabled = false;
        enableCursor.Raise(true);
        string resultName = await m_markingPanel.RenameAndMarkClue(note);
        
        if (!string.IsNullOrEmpty(resultName))
        {
            if (_markedClues.TryAdd(guid, note))
            {
                note.displayName = resultName; 
                markedClueEvent.Raise(note);
            }
        }
       
        _isMarking = false;
        inputReaderNoteBook.SetEnable();
        _virtualMouse.enabled = true;
        enableCursor.Raise(true);
    }

   

    public void RemoveClue(SerializableGuid guid)
    {
        if (_markedClues.Remove(guid))
        {
         
        }
    }
    public bool IsMarked(SerializableGuid guid) => _markedClues.ContainsKey(guid);
    #endregion
    
    private readonly Dictionary<SerializableGuid,Note> _allNotes = new();
    
    private readonly Dictionary<NoteType, List<SerializableGuid>> _notesByTypeIndex = new()
    {
        { NoteType.Log, new List<SerializableGuid>() },
        { NoteType.Objects, new List<SerializableGuid>()}
    };
    
    public List<SerializableGuid> GetGuidsByType(NoteType type) => _notesByTypeIndex[type];
    
    #region Log
    private readonly Dictionary<NpcIdentity, List<SerializableGuid>> _npcLogIndex = new();
    public void AddToNpcIndex(NpcIdentity speaker, SerializableGuid guid)
    {
        if (!_npcLogIndex.TryGetValue(speaker, out var list))
        {
            list = new List<SerializableGuid>();
            _npcLogIndex.Add(speaker, list);
        }
        list.Add(guid);
    }
    public void RemoveFromNpcIndex(NpcIdentity speaker, SerializableGuid guid)
    {
        if (!_npcLogIndex.TryGetValue(speaker, out var list)) return;
        list.Remove(guid);
        if (list.Count == 0) _npcLogIndex.Remove(speaker);
    }
    public Dictionary<NpcIdentity, List<SerializableGuid>> GetNpcLogIndex() => _npcLogIndex;
    
    #endregion
    
    #region  Item

    
    private readonly Dictionary<SerializableGuid, HashSet<string>> _unlockedPoisByItem = new(); // punto de interes
    private readonly Dictionary<Item, string> _unlockedFlashbackNote = new();
    #endregion
    
    #endregion

    #region Component

    private UVVirtualMouse _virtualMouse;
    private CancellationTokenSource _cts;

    #endregion

    #region Button Strategies
    private Dictionary<NoteType, INoteButtonStrategy> _strategies;

    private void InitStrategies()
    {
        _strategies = new Dictionary<NoteType, INoteButtonStrategy>
        {
            { NoteType.Objects, new SingleNoteStrategy(NoteType.Objects, this) },
            { NoteType.Log, new NpcGroupNoteStrategy(this) } 
        };
    }
    
    #endregion

    private void Start()
    {
        m_view = Instantiate(m_view,transform);
        m_markingPanel = Instantiate(m_markingPanel,transform);
        inputReaderNoteBook.Close += Close;
        m_recordNote.OnEventRaised += Record;
        m_openNotebookChannel.OnEventRaised += Open;
        
        InitStrategies();
        _virtualMouse = GetComponentInChildren<UVVirtualMouse>();
    }

    private void Record(Note note)
    {
        if (!_allNotes.TryAdd(note.guid, note)) return;
        if (!_notesByTypeIndex.ContainsKey(note.type)) _notesByTypeIndex[note.type] = new List<SerializableGuid>();
        _notesByTypeIndex[note.type].Add(note.guid);
        
        note.OnRecord(this);
    }

    private void Remove(Note note)
    {
        if (note == null) return;
        if (_notesByTypeIndex.TryGetValue(note.type, out var guidList))
        {
            guidList.Remove(note.guid);
        }
    }
    
    

    private void Open()
    {
        pushEvent.Raise(this);
        _ = ActionTimer.Instance.m_view.DisplayUI();       
    }

    private void Close()
    {
        popEvent.Raise();
      
    }


    #region POI

    public void UnlockPoi(Item item, string poiId)
    {
        if (!_unlockedPoisByItem.ContainsKey(item.guid))
        {
            _unlockedPoisByItem[item.guid] = new HashSet<string>();
        }
        
       
        var set = _unlockedPoisByItem[item.guid];
        
        bool wasCompleteBefore = HasAllPois(item);
        if (!set.Add(poiId)) return;
       
        bool isCompleteNow = HasAllPois(item);
        if (wasCompleteBefore != isCompleteNow)
        {
            m_udpatePoi.Raise(isCompleteNow);
        }
    }
    
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
    
    #endregion


    #region Flashback
    public void UpdateFlashbackInfo(Item item, string info)
    {
        if (!_unlockedFlashbackNote.TryAdd(item, info)) return;
    }

    public string GetItemFlashbackInfo(Item item)
    {
        return !_unlockedFlashbackNote.TryGetValue(item, out var flashback) ? string.Empty : flashback;
    }

    #endregion


    #region Core Logic
    
    private void OpenNotebookByType(NoteType type)
    {
        _currentNoteType = type;
        ResetCts();
        
        m_view.ClearDetail();
        m_view.ClearButton();
        m_view.SetTitle(type.ToString()); // si vamos a hacer localization ya no deberia usar de esta forma , lo hago asi para ahorrarme tiempo

        if (_strategies.TryGetValue(type, out var strategy))
        {
            strategy.Render(m_view, _allNotes, (button, note) => 
            {
                ResetCts();
                _ = SelectNote(button, note, _cts.Token);
            });
        }
    }
    
    #region Button
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

    
    private void ResetCts()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
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
    #endregion

    #endregion
    
    


    private void AddDetailButtons(ButtonFactoryObject parent, NotebookView view, Note note)
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

                var button = m_view.CreateButton($"No saved conversations.");
                button.transform.parent = parent.transform.parent;
              
                button.SetInteractable(false);
                button.MoveToPosition(parent.GetPosition());
                button.gameObject.transform.localScale *= 0.9f;
             
            }
            else _allNotes.Remove(note.guid);
            
        });
    }

    #region IActivity
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderNoteBook.SetEnable();
       
        _virtualMouse.enabled = true;
       
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
        _virtualMouse.enabled = false;
        m_view.gameObject.SetActive(false);
        m_view.RemoveNext();
        m_view.RemovePrevious();
        enableCursor.Raise(false); putInNotebookChannel.Raise();
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
     
    public bool CheckNote(SerializableGuid guid) => _allNotes.ContainsKey(guid);

   
}

public interface IMark
{
    void MarkClue(SerializableGuid guid);
    void RemoveClue(SerializableGuid guid);
    bool IsMarked(SerializableGuid guid);
}

public interface INoteButtonStrategy
{
    void Render(NotebookView view, Dictionary<SerializableGuid, Note> allNotes, Action<ButtonFactoryObject, Note> onSelect);
}

public class NpcGroupNoteStrategy: INoteButtonStrategy
{
    private readonly IMark _markHandler;

    public NpcGroupNoteStrategy(IMark markHandler)
    {
        _markHandler = markHandler;
    }
    public void Render(NotebookView view, Dictionary<SerializableGuid, Note> allNotes, Action<ButtonFactoryObject, Note> onSelect)
    {
        var npcIndex = NotebookManager.Instance.GetNpcLogIndex();

        foreach (var entry in npcIndex)
        {
            NpcIdentity npc = entry.Key;
            List<SerializableGuid> logs = entry.Value;
            if(logs.Count == 0) continue;
            
            var npcFolder = view.CreateButton($"{npc.npcName} ({logs.Count})");

            
            bool isOpen = false;
            List<ButtonFactoryObject> currentSubButtons = new();
         
            npcFolder.AddListener(() =>
            {
                if (isOpen)
                {
                    foreach (var subBtn in currentSubButtons) FlyweightFactory.Instance.Return(subBtn); 
                    
                    currentSubButtons.Clear();
                    isOpen = false;
                }
                else
                {
                    foreach (var logId in logs)
                    {
                        if (!allNotes.TryGetValue(logId, out var note)) continue;
                        
                        bool markedStatus = _markHandler.IsMarked(note.guid);

                        var button = view.CreateToggleButton(
                            note.GetButtonText(),
                            doAction: () => _markHandler.MarkClue(note.guid),
                            undoAction: () => _markHandler.RemoveClue(note.guid),
                            toggle: markedStatus
                        );
                        
                        button.AddListener(() => onSelect(button, note));
                        button.MoveToPosition(npcFolder.GetPosition() + 1);
                        currentSubButtons.Add(button);
                    }
                    
                    isOpen = true;
                }
            });

        }
    }
}

public class SingleNoteStrategy : INoteButtonStrategy
{
    
    private readonly NoteType _targetType;
    private readonly IMark _markHandler;
    public SingleNoteStrategy(NoteType type, IMark markHandler)
    {
        _targetType = type;
        _markHandler = markHandler;
    }
    
    public void Render(NotebookView view, Dictionary<SerializableGuid, Note> allNotes, Action<ButtonFactoryObject, Note> onSelect)
    {
        var guids = NotebookManager.Instance.GetGuidsByType(_targetType);

        foreach (var guid in guids)
        {
            if (!allNotes.TryGetValue(guid, out var note)) continue;
            bool isAlreadyMarked = _markHandler.IsMarked(guid);
            var button = view.CreateToggleButton(
                note.GetButtonText(),
                doAction: () => _markHandler.MarkClue(guid),
                undoAction: () => _markHandler.RemoveClue(guid),
                toggle: isAlreadyMarked
            );
            
            
            button.AddListener(()=> onSelect(button, note));
        }
    }
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
    public List<Proof> isProof;

    public Note(string displayName,  List<Proof> proof = null)
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
    public virtual string GetInfo() => null;
    public virtual void OnRecord(NotebookManager manager) { }
    public virtual void OnRemove(NotebookManager manager) { }
}

public class LogNote : Note
{
    private readonly string _fullInfo;   
    private string _recordInfo;
    private bool _showingFull  = false;
    public NpcIdentity Speaker;
    public LogNote(string displayName,string fullInfo, string recordInfo, NpcIdentity speaker,List<Proof> proof = null) : base(displayName, proof)
    {
        _fullInfo = fullInfo;
        _recordInfo = recordInfo;
        Speaker = speaker;
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
        await RefreshDisplay(view, token);
    }

    public override void OnRecord(NotebookManager manager)
    {
        manager.AddToNpcIndex(Speaker, guid);
    }

    public override void OnRemove(NotebookManager manager)
    {
        manager.RemoveFromNpcIndex(Speaker,guid);
    }
    private async UniTask RefreshDisplay(NotebookView view, CancellationToken token)
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
            _ = RefreshDisplay(view, token);
        });

    }
    public override string GetInfo() => _fullInfo;
    
}

public class ItemNote : Note
{
    private readonly Item _item;
    public ItemNote(string displayName,Item item, List<Proof> proof = null) : base(displayName, proof)
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
