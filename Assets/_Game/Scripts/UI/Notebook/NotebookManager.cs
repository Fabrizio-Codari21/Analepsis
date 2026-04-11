using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public class NotebookManager : Singleton<NotebookManager>, IActivity
{
    
    #region  Inputs & Cursor

    [SerializeField] private CCInputReader inputReader;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private NoteBookInputReader inputReaderNoteBook;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;

    #endregion

    [SerializeField]private NotebookView m_view;
    [SerializeField] private RecordNoteEvent m_recordNote;
    private CancellationTokenSource _cts;
    private readonly Dictionary<SerializableGuid,Note> _notebookPages = new();
    [ReadOnly,ShowInInspector] private NoteType _currentNoteType;
    [ReadOnly, ShowInInspector] public Dictionary<SerializableGuid, Note> markedClues = new();
    [SerializeField] MarkClueEvent markedClueEvent;
    private void Start()
    {
        m_view = Instantiate(m_view,transform);
        inputReader.OpenNotebook += Open;
        inputReaderNoteBook.Close += Close;
        m_recordNote.OnEventRaised += Record;
        markedClueEvent.OnEventRaised += MarkClue;
    }

    private void Record(Note note)
    {
        _notebookPages.TryAdd(note.guid, note);
        MarkClue(note); //esto es temporal
    }

    private void MarkClue(Note note)
    {
        markedClues.TryAdd(note.guid, note);
    }

    private void Open()
    {
        pushEvent.Raise(this);     
    }

    private void Close()
    {
        popEvent.Raise();
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
        enableCursor.Raise(true);
        inputReaderNoteBook.SetEnable();
        m_view.gameObject.SetActive(true);
        m_view.NextButtonAdd(()=>ChangeType(1));
        m_view.PreviousButtonAdd(()=>ChangeType(-1));
        inputReaderNoteBook.Flip += ChangeType;
        
        OpenNotebookByType(_currentNoteType);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        enableCursor.Raise(false);
        inputReaderNoteBook.SetEnable(false);
        m_view.gameObject.SetActive(false);
        m_view.RemoveNext();
        m_view.RemovePrevious();
       
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
    public List<TheoryboardManager.Whodunnit> isProof;

    protected Note(string displayName, List<TheoryboardManager.Whodunnit> proof = default)
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
    public LogNote(string displayName, string info, List<TheoryboardManager.Whodunnit> proof = default) : base(displayName, proof)
    {
        _info = info;
        type =  NoteType.Log;
    }
    public override async UniTask Show(NotebookView view, CancellationToken token)
    { 
       await view.PlayText(_info, token);
    }
}

public class ItemNote : Note
{
    private readonly Item _item;
    public ItemNote(string displayName,Item item, List<TheoryboardManager.Whodunnit> proof = default) : base(displayName, proof)
    {
        _item =  item;
        type = NoteType.Objects;
        guid = _item.guid;  // para usa el guid de item para que solamente anota el item y no se repite, pero el clue se puede ir desbloqueando de a poco
    }

    public override async UniTask Show(NotebookView view, CancellationToken token)
    {
        view.CreateImage(_item.sprite);
        await view.PlayText(_item.Description, token);
    }
}
