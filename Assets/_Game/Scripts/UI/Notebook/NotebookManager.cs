using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

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
    [SerializeField] private NoteEvent note; // record 
    [SerializeField] private BoolEventChannel m_updatePoi;
    
    #endregion
    
    
    #region General
    [SerializeField] private NotebookRepresenter representer;
    [ReadOnly,ShowInInspector] private PageType _currentPageType;
    
    
    #endregion
    
    
    #region Item Page
    private readonly Dictionary<SerializableGuid, HashSet<string>> _unlockedPoisByItem = new(); // punto de interes
    private readonly Dictionary<Item, string> _unlockedFlashbackNote = new();
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

    [Header("Character Data")]
    [ShowInInspector,ReadOnly] public HashSet<NpcIdentity> FoundCharacters { get; } = new();
    [SerializeField] private NpcEvent m_onNpcFound;

 
    public void AddCharacter(NpcIdentity npc)
    {
        if (!FoundCharacters.Add(npc)) return;
        m_onNpcFound?.Raise(npc);
    }
    
    public List<DialogueNote> GetDialoguesFor(NpcIdentity npcIdentity)
    {
        if (_npcTalkedDialogue != null && _npcTalkedDialogue.TryGetValue(npcIdentity, out var list))
        {
            return list;
        }
        return new List<DialogueNote>(); 
    }
    #endregion


    #region  Unity Life

    protected override void Awake()
    {
        base.Awake();
        representer = Instantiate(representer);
        representer.Initialize(this);
    }
 
    private void Start()
    {
      
       
        inputReaderNoteBook.Close += Close;
        note.OnEventRaised += Record;
        m_openNotebookChannel.OnEventRaised += Open;
        
    }

    private void OnDestroy()
    {
        inputReaderNoteBook.Close -= Close;
        note.OnEventRaised -= Record;
        m_openNotebookChannel.OnEventRaised -= Open;
    }

    #endregion
    
    private readonly HashSet<SerializableGuid> _allNote = new();
    private void Record(Note note)
    {
        if (!_allNote.Add(note.guid))
        {
           
        }
  
    }  
    
    [ShowInInspector, ReadOnly] private Dictionary<NpcIdentity,List<DialogueNote>> _npcTalkedDialogue = new();
    public  void RecordDialogueProgress(NpcIdentity npc, Dialogue dialogue, INode currentNode, INode parentNode)
    {
        if (dialogue == null || currentNode == null) return;
        
        if (!_npcTalkedDialogue.TryGetValue(npc, out var dialogueList))
        {
            dialogueList = new List<DialogueNote>();
            _npcTalkedDialogue[npc] = dialogueList;
        }
        
        var dialogueNote = dialogueList.Find(x => x.GetFullDialogue() == dialogue); // encuetro si ya existia el node
        
        if (dialogueNote == null)
        {
            Whodunnit w = new Whodunnit();
            if (currentNode is DialogueNode dialogueNode)
            {
                w = dialogueNode.doesItProveAnything;
            }
            dialogueNote = new DialogueNote(dialogue.name, dialogue,new Tuple<Clue, List<Whodunnit>>(dialogue,new List<Whodunnit>() {w}));
            dialogueList.Add(dialogueNote);
        }
        dialogueNote.RegisterNodeVisit(currentNode, parentNode);
      
    }

    
    #region Open & Close
    private void Open()
    {
        pushEvent.Raise(this);
        AudioManager.Instance.SelectSFX(SFXType.Player, "Open");
        takeOutNotebookChannel.Raise(representer);
        ShowLayout(0);
        
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
            case 0: return;
            case > 0: NextPage(); return;
            case < 0: PreviousPage(); break;
        }
    }
    
    
    #region Page Control
    
    #region Switch
    
    private readonly List<NotebookLayout> _layouts = new();
    [ShowInInspector,ReadOnly] private NotebookLayout _currentLayout;
    private int _currentIndex;
    private Dictionary<int, NotebookLayout> _layoutsDictionary = new ();
    private void NextPage() 
    {
        if (_layouts.Count == 0) return;

        int next = _currentIndex + 1;

        if (next >= _layouts.Count) next = 0;

        ShowLayout(next);
    }

    private void PreviousPage()
    {
        if (_layouts.Count == 0) return;

        int prev = _currentIndex - 1;

        if (prev < 0) prev = _layouts.Count - 1;

        ShowLayout(prev);
    }
    
    #endregion
    
    #endregion
    
    #region Layout
    public void AddLayout(NotebookLayout layout)
    {
        int index =  _layouts.Count;
        layout.index = index;
        _layouts.Add(layout);
        _layoutsDictionary.Add(index, layout);
    }

    private void ShowLayout(int index)
    {
        if(_layouts.Count == 0) return;
        if (index < 0 || index >= _layouts.Count){ return;}
        if (_currentLayout == _layouts[index])
        {
            _currentLayout.Show(); 
            return;
        }
        _currentLayout?.Hide();
        _currentIndex =  index;
        _currentLayout = _layouts[index];
        _currentLayout.Show();
    }

    public void TryShowLayoutFor(NotebookLayout layout)
    {
        Debug.Log("Try Show  Layout For "  + layout.gameObject.name);
        ShowLayout(layout.index);
    }

    #endregion
    
    
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
            if (unlockedIds.Contains(poiData.poiId)) descriptions.Add(poiData.description);
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

    public bool CheckNote(SerializableGuid guid) => _allNote.Contains(guid);
    
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
    Angry,
}

[Serializable]
public  class Note
{
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public PageType type;
    public string displayName;
    public Tuple<Clue,List<Whodunnit>> IsProof;
    public Note(string displayName,  Tuple<Clue,List<Whodunnit>> proof = null)
    {
        this.displayName = displayName;
        IsProof = proof;
    }

    public virtual string GetButtonText()
    {
        return displayName;
    }
    public virtual UniTask Show(NotebookRepresenter representer, CancellationToken token)
    {
        return UniTask.CompletedTask;
    }
    public virtual string GetInfo() => null;
}


public class ItemNote : Note
{
    private readonly Item _item;
    public ItemNote(string displayName,Item item, Tuple<Clue, List<Whodunnit>> proof = null) : base(displayName, proof)
    {
        type = PageType.Objects;
        if (item == null) return;
        _item =  item;
        guid = _item.guid;  
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
    

    // public override  UniTask Show(NotebookRepresenter representer, CancellationToken token)
    // {
    //     // representer.CreateImage(_item.sprite);
    //     //
    //     //
    //     // await representer.PlayText(FullInfo(), token);
    // }
    public override string GetInfo() => FullInfo().AsString();
}


public class TreeNode
{
    public readonly INode Source;

    public TreeNode Parent;

    public readonly List<TreeNode> Children = new();

    // Final position
    public float X;
    public float Y;

    // Modifier
    public float Mod;

    // Buchheim fields
    public float Shift;
    public float Change;

    public TreeNode Thread;
    public TreeNode Ancestor;

    // Order index among siblings
    public int Number;

    public readonly bool IsLocked;
  

    public bool IsLeaf => Children.Count == 0;

    public TreeNode(INode source,  bool isLocked = false)
    {
      
        Source = source;
        IsLocked = isLocked;

        X = 0;
        Y = 0;
        Mod = 0;

        Shift = 0;
        Change = 0;

        Thread = null;
        Ancestor = this;
    }

    public TreeNode GetLeftSibling()
    {
        if (Parent == null || Number == 0) return null; 
        return Parent.Children[Number - 1];
    }

    public TreeNode GetLeftMostSibling()
    {
        if (Parent == null || Parent.Children.Count == 0) return null;

        return Parent.Children[0] == this ? null : Parent.Children[0];
    }

    public TreeNode GetNextLeft()
    {
        return IsLeaf ? Thread : Children[0];
    }

    public TreeNode GetNextRight()
    {
        return IsLeaf ? Thread : Children[^1];
    }
}


public class DialogueNote : Note
{
    private readonly Dialogue _dialogueRepresenter;
    private readonly HashSet<SerializableGuid> _visitedRawNodeGuids = new();
    private readonly Dictionary<SerializableGuid, TreeNode> _rtNodeLookup = new();
    public TreeNode RuntimeTreeRoot { get; private set; }
    
    private List<string> _fullInfo;

    public DialogueNote(string displayName, Dialogue dialogueRepresenter,  Tuple<Clue, List<Whodunnit>> proof = null) : base(displayName, proof)
    {
        _dialogueRepresenter = dialogueRepresenter;
        type = PageType.Character;
        if (_dialogueRepresenter != null && _dialogueRepresenter.startingNode != null)
        {
            InitRoot(_dialogueRepresenter.startingNode);
        }
    }
    
    public override string GetInfo() => _fullInfo.AsString();
    public Dialogue GetFullDialogue() => _dialogueRepresenter;
    
    private void InitRoot(DialogueNode startingNode)
    {
        RuntimeTreeRoot = new TreeNode( startingNode);
        _visitedRawNodeGuids.Add(startingNode.guid);
        _rtNodeLookup[startingNode.guid] = RuntimeTreeRoot;
    }
    
    public void RegisterNodeVisit(INode currentNode, INode parentNode)
    {
        if (currentNode == null) return;
        SerializableGuid currentGuid = GetNodeGuid(currentNode);
        
        if (_visitedRawNodeGuids.Contains(currentGuid)) return;
        SerializableGuid parentGuid = parentNode != null ? GetNodeGuid(parentNode) : SerializableGuid.Empty;
        if (!_rtNodeLookup.TryGetValue(parentGuid, out var rtParent))
        {
            return;
        }
        bool isNpc = currentNode is DialogueNode;
        
        TreeNode rtChild = new TreeNode(currentNode, isNpc)
        {
            Parent = rtParent
        };
        
        rtParent.Children.Add(rtChild);
        
        _visitedRawNodeGuids.Add(currentGuid);
        _rtNodeLookup[currentGuid] = rtChild;
    }
    public bool IsNodeUnlocked(SerializableGuid nodeGuid) => _visitedRawNodeGuids.Contains(nodeGuid);
    private SerializableGuid GetNodeGuid(INode node)
    {
        if (node is DialogueNode dn) return dn.guid;
        
        if (node is DialogueResponse dr) return dr.nextNode?.guid ?? SerializableGuid.NewGuid();
        
        return SerializableGuid.Empty;
    }
}