using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.PlayerLoop;
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
    [SerializeField] private EventChannel takeOutNotebookChannel;
    [SerializeField] private EventChannel putInNotebookChannel;
    [SerializeField] private NotebookView m_view;
    [SerializeField] private RecordNoteEvent m_recordNote;
    [SerializeField] private BoolEventChannel m_udpatePoi;
    private CancellationTokenSource _cts;
    private readonly Dictionary<SerializableGuid,Note> _notebookPages = new();
    [ReadOnly,ShowInInspector] private NoteType _currentNoteType;
    [ReadOnly, ShowInInspector] public Dictionary<SerializableGuid, Note> markedClues = new();
    
    private readonly Dictionary<SerializableGuid, HashSet<string>> _unlockedPoisByItem = new();

    private readonly Dictionary<Item, string> _unlockedFlashbackNote = new();

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
    [SerializeField] MarkClueEvent markedClueEvent;
    private void Start()
    {
        m_view = Instantiate(m_view,transform);
        inputReaderNoteBook.Close += Close;
        m_recordNote.OnEventRaised += Record;
        markedClueEvent.OnEventRaised += MarkClue;
        m_openNotebookChannel.OnEventRaised += Open;
    }

    private void Record(Note note)
    {
        if (!_notebookPages.TryAdd(note.guid, note))
        {
            Debug.Log("Has Note");
        }
  
        //MarkClue(note); //esto es temporal
    }

    private void MarkClue(Note note)
    {
        if(!markedClues.Remove(note.guid)) markedClues.TryAdd(note.guid, note);

        print("Marked clue: " + note.displayName);
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
        m_view.SetTitle(type.ToString()); // si vamos a hacer localization ya deberia usar de esta forma , lo hago asi para ahorrarme tiempo
        if(_notebookPages.Where(x => x.Value.type == type).Count() <= 0)
        {
            var button = m_view.CreateButton($"No {type} found yet."); 
            button.DisableSub(); return;
        }
        foreach (var note in _notebookPages.Values)
        {
            if(note.type != type) continue;
            var cachedNote = note;
            
            var button = m_view.CreateButton(cachedNote.GetButtonText());
            button.AddListener(() =>
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                m_view.ClearDetail();
                _ = SelectNote(cachedNote,_cts.Token);
            });
            //button.MoveSubToLast();
            button.EnableSub();
            button.AddListenerToSub(() =>
            {
                //_cts?.Cancel();
                //_cts?.Dispose();
                //_cts = new CancellationTokenSource();
                markedClueEvent.Raise(note);
                button.DisplayMark(markedClues.ContainsKey(note.guid));
            });
        }
    }
    private async UniTask SelectNote(Note note, CancellationToken token)
    {
        try
        {
            m_view.ClearDetail();
            await note.Show(m_view, token);
            token.ThrowIfCancellationRequested();
            var button = m_view.CreateDetailButton("Clear");
            button.AddListener(() =>
            {
               m_view.ClearDetail();
            });
        }
        catch (OperationCanceledException)
        {
        }
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

[Serializable]
public abstract class Note
{
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public NoteType type;
    public string displayName;
    public List<Whodunnit> isProof;

    protected Note(string displayName, List<Whodunnit> proof = null)
    {
        this.displayName = displayName;
        this.isProof = proof;
    }

    public virtual string GetButtonText()
    {
        return displayName;
    }
    public abstract UniTask Show(NotebookView view, CancellationToken token);
}

public class LogNote : Note
{
    private readonly string _info;
    
    public LogNote(string displayName, string info, List<Whodunnit> proof = null) : base(displayName, proof)
    {
        _info = info;
        type =  NoteType.Log;
    }
    public override async UniTask Show(NotebookView view, CancellationToken token)
    { 
       await view.PlayText(new(){_info}, token);
    }
}

public class ItemNote : Note
{
    private readonly Item _item;
    public ItemNote(string displayName,Item item, List<Whodunnit> proof = null) : base(displayName, proof)
    {
        _item =  item;
        type = NoteType.Objects;
        guid = _item.guid;  // para usa el guid de item para que solamente anota el item y no se repite, pero el clue se puede ir desbloqueando de a poco
    }

    public override async UniTask Show(NotebookView view, CancellationToken token)
    {
        view.CreateImage(_item.sprite);
        List<string> fullContent = new List<string>();
        var unlockedDescriptions = NotebookManager.Instance.GetUnlockedPoiDescriptions(_item);
        
        foreach (var desc in unlockedDescriptions)
        {
            fullContent.Add($"{unlockedDescriptions.IndexOf(desc) + 1})  {desc}"); 
        }

        var unlockedFlash = NotebookManager.Instance.GetItemFlashbackInfo(_item);
        if(unlockedFlash != string.Empty) fullContent.Add($"FLASHBACK :  {unlockedFlash}");
    
        
        await view.PlayText(fullContent, token);
    }
}
