using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DialogueTree : MonoBehaviour
{
    #region Variables
    public ScrollRect treeScroll;

    [Header("World Space Movement & Zoom")]
    public float scrollSpeed = 25f;
    public float zoomSpeed = 5f;

    [Header("Tree Layout Geometry (Pure World Units)")]
    [SerializeField] private float levelVerticalDistance = 40.0f;
    [SerializeField] private float baseHorizontalSpacing = 30.0f;
    [SerializeField] private float spacingAttentuation = 0.85f;
    [SerializeField] private float minSpacing;

    public DialogueTreeUI contentUI;
    public GameObject treeAnchor;
 
    public Transform treeParent, textParent, characterParent;
    public Image lockImage;
    public Color _lockColor = new(0.4f, 0, 0.1f, 0.1f);
    
    [Header("UI REFERENCES")]
    [Header("Tree")]
    [SerializeField] private RectTransform nodeContainerPrefab; 
    [SerializeField] private ButtonSetting buttonSetting;
    [SerializeField] private ImageSelector arrowImage;
    
    [Header("Events")]
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;

    NotebookManager _manager;

    [Header("Components")]
    private NotebookRepresenter representer;
    [SerializeField] private UIHoverDetector scrollHoverDetector; 
    [SerializeField] private Vector3 _scrollScale = Vector3.one;
    [SerializeField] private Vector3 _scrollOffset = Vector3.zero;
    
    [SerializeField] private float angleOffset = 30f;
    #endregion

    private void Awake()
    {
        representer = GetComponentInParent<NotebookRepresenter>();
        _manager = NotebookManager.Instance;
    }

    private void Update()
    {
        MoveTreeScroll();
    }

    public async UniTask ToggleTree(bool on = true, NpcIdentity openingCharacter = null)
    {
        if (on)
        {
            ResetScrollAndScale(); // 打开时重置对焦
            treeAnchor.gameObject.SetActive(true);
            var returnButton = representer.CreateCustomButton("- RETURN -", characterParent, buttonSetting);
            returnButton.DisableSub();
            returnButton.AddListener(async () => { await ToggleTree(false); });
            
            foreach (var character in _manager.FoundCharacters)
            {
                var button = representer.CreateCustomButton(
                    character.Key.npcName,
                    characterParent,
                    buttonSetting);
                
                button.DisableSub();
                button.AddListener(async () =>
                {
                    ClearText();
                    DeleteTree();
                    ResetScrollAndScale(); 
                    await BuildTree(_manager.StartedDialogues[_manager.FoundCharacters.ToList().IndexOf(character)]);
                });
            }
            
            if(openingCharacter != default) await BuildTree(_manager.StartedDialogues[_manager.FoundCharacters.ToList().IndexOf(_manager.FoundCharacters.First(x => x.Key == openingCharacter))]);
        }
        else
        {
            representer.ClearDetail();
            _= representer.ResetNotebookAnimation();
            ClearText(); 
            DeleteTree(); 
            ClearButtons();
            ResetScrollAndScale();
        }
    }

  
    private void ResetScrollAndScale()
    {
        
        if (treeScroll != null)
        {
            treeScroll.verticalNormalizedPosition = 0.5f;
            treeScroll.horizontalNormalizedPosition = 0.5f;
        }
        treeParent.localPosition = Vector3.zero;
        
       
        treeParent.localScale = _scrollScale;
    }

   
    public void MoveTreeScroll()  
    {
        if (!treeAnchor.gameObject.activeInHierarchy || _manager.m_markingPanel.isMarkingClue) return;
        
      
        float moveStep = scrollSpeed * Time.deltaTime;
        Vector3 localMove = Vector3.zero;
    
        if (Input.GetKey(KeyCode.W)) localMove -= new Vector3(0, moveStep, 0);
        else if (Input.GetKey(KeyCode.S)) localMove += new Vector3(0, moveStep, 0);
        if (Input.GetKey(KeyCode.D)) localMove -= new Vector3(moveStep, 0, 0);
        else if (Input.GetKey(KeyCode.A)) localMove += new Vector3(moveStep, 0, 0);
    
        treeParent.localPosition += localMove;

       
        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f && scrollHoverDetector != null && scrollHoverDetector.IsMouseHovering)
        {
            
            Vector3 mouseWorldPosBefore = GetMouseWorldPosOnCanvas();
            Vector3 mouseLocalPosBefore = treeParent.InverseTransformPoint(mouseWorldPosBefore);
           
            float zoomStep = Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
            Vector3 targetScale = treeParent.localScale + new Vector3(zoomStep, zoomStep, 0);

            targetScale.x = Mathf.Clamp(targetScale.x, _scrollScale.x / 2f, _scrollScale.x * 4f);
            targetScale.y = Mathf.Clamp(targetScale.y, _scrollScale.y / 2f, _scrollScale.y * 4f);
        
            treeParent.localScale = targetScale;
            
            Vector3 mouseWorldPosAfter = treeParent.TransformPoint(mouseLocalPosBefore);

        
            Vector3 worldOffset = mouseWorldPosBefore - mouseWorldPosAfter;
            Vector3 localOffset = treeParent.parent.InverseTransformVector(worldOffset); 
        
            treeParent.localPosition += localOffset;
        }

        if (!Input.GetKeyDown(KeyCode.F)) return;
        ResetScrollAndScale();
    }

    private Vector3 GetMouseWorldPosOnCanvas()
    {
        RectTransform parentRect = treeParent.GetComponent<RectTransform>();
        
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, Input.mousePosition, Camera.main, out Vector3 worldPoint))
        {
            return worldPoint;
        }
        
        return treeParent.position;
    }



    #region Tree Generation
    
    
    Dialogue _currentDialogue;
    List<INode> _unlockedDialogue;

    public void DeleteTree()
    {
        foreach(Transform child in treeParent) Destroy(child.gameObject);
    }

    public void ClearText()
    {
        representer.Despawn(textParent);
    }

    public void ClearButtons()
    {
        representer.Despawn(characterParent);
    } 

    public async UniTask BuildTree(DialogueNote dialogue)
    {
        _currentDialogue = dialogue.GetFullDialogue();
        _unlockedDialogue = dialogue.GetUnlockedDialogue();
        await AddLevel(new(){ _currentDialogue.startingNode }, Vector3.zero + _scrollOffset, 1);
    }

    private async UniTask AddLevel(List<DialogueNode> nodes, Vector3 parentLocalPos, int currentLevel = 1)
    {
        int count = nodes.Count;
        if (count == 0) return;
        
        float currentSpacing = baseHorizontalSpacing * Mathf.Pow(spacingAttentuation, currentLevel - 1);
        float halfTotalWidth = (count - 1) * currentSpacing / 2f;
        
        List<(DialogueNode node, Vector3 localPos)> spawnedNodesInThisLevel = new();

        for (int i = 0; i < count; i++)
        {
            DialogueNode node = nodes[i];
            Vector3 nodeLocalPos;

            if (currentLevel == 1)
            {
                nodeLocalPos = Vector3.zero + _scrollOffset;
            }
            else
            {
                float targetX = parentLocalPos.x - halfTotalWidth + (i * currentSpacing);
                float targetY = parentLocalPos.y - levelVerticalDistance;
                nodeLocalPos = new Vector3(targetX, targetY, 0f);
            }

            if (currentLevel > 1)
            {
                Vector3 arrowLocalPos = Vector3.Lerp(parentLocalPos, nodeLocalPos, 0.5f);
                ImageSelector arrow = Instantiate(arrowImage, treeParent);
                arrow.transform.localScale = Vector3.one;
                arrow.transform.localPosition = new Vector3(arrowLocalPos.x, arrowLocalPos.y, 0f);
                arrow.gameObject.name = $"Arrow_{currentLevel - 1}_Branch_{i + 1}";
                arrow.SetRandomSprite();
                
                Vector3 direction = nodeLocalPos - parentLocalPos;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.localRotation = Quaternion.Euler(0f, 0f, angle - angleOffset ); 

                if (node.PreviousResponse != null && !node.PreviousResponse.IsAvailable())
                {
                    arrow.baseImage.color = _lockColor;
                }
            }
            
            LogNote note = new LogNote(
                node.PreviousResponse != null ? node.PreviousResponse.responseText : "Beginning",
                new() { node.dialogueText },
                new() { node.dialogueText },
                new(_currentDialogue, new() { node.doesItProveAnything }));

            if (node.PreviousResponse != null && !node.PreviousResponse.IsAvailable())
            {
                Image locked = Instantiate(lockImage, treeParent);
                locked.transform.localScale = Vector3.one;
                locked.transform.localPosition = nodeLocalPos;
                locked.transform.localRotation = Quaternion.identity;
                locked.gameObject.name = $"Lock_{currentLevel}_Branch_{i + 1}";
                locked.GetComponentInChildren<Image>().color = _lockColor;
                continue;
            }

            bool unread = !_unlockedDialogue.Contains(node);
            ButtonFactoryObject button = SpawnClueButton(note, treeParent, unread);
            button.transform.localPosition = nodeLocalPos;
            button.gameObject.name = $"Button_{currentLevel}_Branch_{i + 1}";

            if (node.responses.Count > 0 && !unread)
            {
                var nextNodes = node.responses
                    .Where(r => r.nextNode != null)
                    .Select(x => { x.nextNode.PreviousResponse = x; return x.nextNode; })
                    .ToList();
                
                if (nextNodes.Count > 0)
                {
                    spawnedNodesInThisLevel.Add((node, nodeLocalPos));
                }
            }
        }

        foreach (var item in spawnedNodesInThisLevel)
        {
            var nextNodes = item.node.responses
                .Where(r => r.nextNode != null)
                .Select(x => x.nextNode)
                .ToList();

            await AddLevel(nextNodes, item.localPos, currentLevel + 1);
        }
    }

    public ButtonFactoryObject SpawnClueButton(Note cachedNote, Transform parent, bool unread = false)
    {
        var button = representer.CreateCustomButton(cachedNote.GetButtonText(), parent, buttonSetting);
        button.transform.localScale = Vector3.one;
        button.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-5f, 5f));
        
        if (unread)
        {
            button.SetInteractable(false);
            button.DisableSub();
            button.SetText("???");
            return button;
        }
        else
        {
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            text.fontSizeMax = 7;
            text.fontSizeMin = 6;
        }
        
        if (_manager.markedClues.ContainsKey(cachedNote.guid))
        {
            button.DisplayMark(true);
        }
        button.AddListener(async () =>
        {
            var newToken = _manager.Cancel();
            ClearText();
            await contentUI.PlayText(new(){cachedNote.GetInfo()}, CancellationToken.None, textParent, 6);
            _manager.AddDetailButtons(button, representer, cachedNote);
        });

        button.EnableSub();
        _manager.enableButtonsEvent += (x) =>
        {
            if (button != null) button.EnableSub();
        };
        button.AddListenerToSub(() =>
        {
            if (_manager.markedClues.ContainsKey(cachedNote.guid))
            {
                button.DisplayMark(false);
                _manager.markedClues.Remove(cachedNote.guid);
                return;
            }
            _manager.m_markingPanel.isMarkingClue = true;
        
            button.DisplayMark(true);
            _manager.ClearMarkEvent();
            _manager.enableMarkEvent += button.DisplayMark;
            _manager.EnableButtons(false);
            _manager.markedClueEvent.Raise(cachedNote);
        });
        
        return button;
    }
    #endregion
}