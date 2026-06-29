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
    [SerializeField] private EventChannel m_refreshTree;
    [SerializeField] private BoolEventChannel m_showAction;
    #endregion
    
    #region General
    [SerializeField] private NotebookRepresenter representer;
    [ReadOnly,ShowInInspector] private PageType _currentPageType;
    [SerializeField] private NpcEvent m_onCharacterSelected;
    
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
    [ShowInInspector,ReadOnly] public HashSet<NpcIdentity> FoundCharacters { get; } = new(); // Save Data
    [ShowInInspector,ReadOnly] public HashSet<SerializableGuid> FoundCharacterGuids { get; } = new();
    [SerializeField] private NpcEvent m_onNpcFound;
    
    public void AddCharacter(NpcIdentity npc)
    {
        if (npc == null) return;

        if (!FoundCharacterGuids.Add(npc.npcGuid)) return;
        bool isFirstCharacter = FoundCharacters.Count == 0;
        FoundCharacters.Add(npc);
        
        m_onNpcFound?.Raise(npc);

        if (isFirstCharacter)
        {
            m_onCharacterSelected?.Raise(npc);
        }
    }
    
    public List<DialogueNote> GetDialoguesFor(NpcIdentity npcIdentity)  
    {
        if (_npcTalkedDialogue != null && _npcTalkedDialogue.TryGetValue(npcIdentity.npcGuid, out var list))
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
    private void Record(Note n)
    {
        if (!_allNote.Add(n.guid))
        {
            
        }
    }  
    
  
    [ShowInInspector, ReadOnly] private Dictionary<SerializableGuid,List<DialogueNote>> _npcTalkedDialogue = new();
    public  void RecordDialogueProgress(SerializableGuid id, Dialogue dialogue, INode currentNode, INode parentNode)
    {
        if (dialogue == null || currentNode == null) return;
        
        if (!_npcTalkedDialogue.TryGetValue(id, out var dialogueList))
        {
            dialogueList = new List<DialogueNote>();
            _npcTalkedDialogue[id] = dialogueList;
        }
        
        DialogueNote dialogueNote = dialogueList.Find(x => x.GetFullDialogue() == dialogue); // encuetro si ya existia el node
        
        if (dialogueNote == null)
        {
            dialogueNote = new DialogueNote(dialogue.name, dialogue);
            dialogueList.Add(dialogueNote);
        }
        dialogueNote.RegisterNodeVisit(currentNode, parentNode);
      
    }

    
    #region Open & Close
    private void Open()
    {
        if(FlashbackManager.Instance.InFlashbackState()) return;
        pushEvent.Raise(this);
        AudioManager.Instance.SelectSfx(SFXType.Player, "Open");
        takeOutNotebookChannel.Raise(representer);
        m_showAction?.Raise(true);
        ShowLayout(0);
        
    }

    private void Close()
    {
        popEvent.Raise();
        m_showAction?.Raise(false);
        AudioManager.Instance.SelectSfx(SFXType.Player, "Close");
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

    public bool UnlockPoi(Item item, string poiId)
    {
        if (!_unlockedPoisByItem.ContainsKey(item.guid))
        {
            _unlockedPoisByItem[item.guid] = new HashSet<string>();
        }
        
       
        var set = _unlockedPoisByItem[item.guid];
        
        bool wasCompleteBefore = HasAllPois(item);
        if (!set.Add(poiId)) return false;
        
       
        bool isCompleteNow = HasAllPois(item);
        if (wasCompleteBefore != isCompleteNow)
        {
            m_updatePoi.Raise(isCompleteNow);
        }

        return true;
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
    
    public Note(string displayName)
    {
        this.displayName = displayName;
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
    public ItemNote(string displayName,Item item) : base(displayName)
    {
        type = PageType.Objects;
        if (item == null) return;
        _item =  item;
        guid = _item.guid;  
    }

    private List<string> FullInfo()
    {
        List<string> fullContent = new List<string>();
        var unlockedDescriptions = NotebookManager.Instance.GetUnlockedPoiDescriptions(_item);

        foreach (var desc in unlockedDescriptions)
        {
            fullContent.Add($"{unlockedDescriptions.IndexOf(desc) + 1})  {desc}\n");
        }

        var unlockedFlash = NotebookManager.Instance.GetItemFlashbackInfo(_item);
        if (unlockedFlash != string.Empty) fullContent.Add($"FLASHBACK :  {unlockedFlash}");
        return fullContent;
    }
    
 
    public override string GetInfo() => FullInfo().AsString();
}
public enum NodeVisualState
{
    Visited,       
    Unchosen,         
    ConditionLocked  
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

    public SerializableGuid RepresentNoteGuid;

    // Order index among siblings
    public int Number;

    public bool IsLocked => VisualState == NodeVisualState.ConditionLocked;
    public bool IsUnchosen => VisualState == NodeVisualState.Unchosen;
    public NodeVisualState VisualState { get; set; } = NodeVisualState.ConditionLocked;

    public bool IsLeaf => Children.Count == 0;
    public RectTransform RuntimeRect { get; set; }

    public TreeNode(INode source, NodeVisualState visualState = NodeVisualState.ConditionLocked)
    {
        VisualState = visualState;
        Source = source;

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
    
    [ShowInInspector, ReadOnly]
    private readonly HashSet<SerializableGuid> _visitedRawNodeGuids = new();
    
    private readonly Dictionary<SerializableGuid, TreeNode> _rtNodeLookup = new();
    public TreeNode RuntimeTreeRoot { get; private set; }
    
    private List<string> _fullInfo;

    public DialogueNote(string displayName, Dialogue dialogueRepresenter) : base(displayName)
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
        RuntimeTreeRoot = new TreeNode(startingNode, NodeVisualState.Visited);
        _visitedRawNodeGuids.Add(startingNode.guid);
        _rtNodeLookup[startingNode.guid] = RuntimeTreeRoot;
    }
    
    public void RegisterNodeVisit(INode currentNode, INode parentNode)
    {
        if (currentNode == null) return;
        
        SerializableGuid currentGuid = GetNodeGuid(currentNode);
        SerializableGuid parentGuid = parentNode != null ? GetNodeGuid(parentNode) : SerializableGuid.Empty;

        // 🌟 【终极修正补丁】：数据上报记录独立化！
        // 不管它能不能在 _rtNodeLookup 里找到前置渲染父级，
        // 只要玩家听到了这句对话，我们高优先级、无条件地将其 Guid 狠狠踩进通关哈希表里！
        if (!currentGuid.Equals(SerializableGuid.Empty))
        {
            _visitedRawNodeGuids.Add(currentGuid);
        }
        
        // 建立树状拓扑图的运行时缓存逻辑，仅作为排布辅助，不再具备“连累、卡死哈希数据更新”的副作用
        if (_rtNodeLookup.TryGetValue(parentGuid, out var rtParent))
        {
            if (!_rtNodeLookup.ContainsKey(currentGuid))
            {
                TreeNode rtChild = new TreeNode(currentNode, NodeVisualState.Visited) { Parent = rtParent };
                rtParent.Children.Add(rtChild);
                _rtNodeLookup[currentGuid] = rtChild;
            }
        }
    }public bool IsNodeUnlocked(SerializableGuid nodeGuid) => _visitedRawNodeGuids.Contains(nodeGuid);
    
    private SerializableGuid GetNodeGuid(INode node)
    {
        if (node is DialogueNode dn) return dn.guid;
        if (node is DialogueResponse dr) return dr.nextNode?.guid ?? SerializableGuid.NewGuid();
        return SerializableGuid.Empty;
    }
    
    // 🌟 配合 TreePage 干净隔离层的数据状态探测分流
    public NodeVisualState GetNodeVisualState(DialogueNode configNode, DialogueResponse runtimePreviousResponse)
    {
        if (configNode == null) return NodeVisualState.ConditionLocked;
        
        // 1. 运行时明确走过 -> 亮起
        if (_visitedRawNodeGuids.Contains(configNode.guid))
        {
            return NodeVisualState.Visited;
        }
        
        // 2. 深度后代激活兜底 -> 亮起
        if (IsAnyChildVisited(configNode))
        {
            return NodeVisualState.Visited;
        }
        
        // 3. 使用 TreePage 传下来的局域清洁关系进行判定
        if (runtimePreviousResponse != null)
        {
            if (!runtimePreviousResponse.IsAvailable())
            {
                return NodeVisualState.ConditionLocked;
            }
            return NodeVisualState.Unchosen;
        }
        
        return NodeVisualState.ConditionLocked;
    }

    // 保持对老结构的向下兼容
    public NodeVisualState GetNodeVisualState(DialogueNode configNode)
    {
        return GetNodeVisualState(configNode, configNode.PreviousResponse);
    }

    private bool IsAnyChildVisited(DialogueNode node)
    {
        if (node == null || node.responses == null) return false;

        foreach (var response in node.responses)
        {
            if (response.nextNode == null) continue;
            if (_visitedRawNodeGuids.Contains(response.nextNode.guid)) return true;
            if (IsAnyChildVisited(response.nextNode)) return true;
        }

        return false;
    }
}