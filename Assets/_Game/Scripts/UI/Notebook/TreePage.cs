using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TreePage : NotebookPage
{
    [Header("Tree Layout Geometry (Pure World Units)")]
    [SerializeField] private float levelVerticalDistance = 60.0f;
    [SerializeField] private float baseHorizontalSpacing = 80.0f; 
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 5f;
    [SerializeField] private float zoomSmoothing = 10f;

    [Header("UI REFERENCES")]
    [Space(5)]
    [Header("Button")]
    [SerializeField] private Transform m_treeRoot;
    [SerializeField] private ButtonSetting m_nodeButton;
    
    [Header("Arrow")]
    [SerializeField] private ImageSelector arrowImage;
    [SerializeField] private float angleOffset = 30f;

    [Header("Multi-Arrow Settings")]
    [SerializeField] private float m_arrowStepDistance = 60f;
    
    [SerializeField, Range(0f, 1f)] private float spreadIntensity = 0.35f;
    [SerializeField] private float extraYDropIntensity = 12f;
    [SerializeField] private float arrowPadding = 15f;

    [Header("Lock Image")]
    [SerializeField] private Color m_lockColor = new (0.4f, 0, 0.1f, 0.1f);
    [SerializeField] private Image m_lockImage;
    
    [Header("Text")]
    [SerializeField] private DynamicTextSetting m_dynamicTextSetting;
    [SerializeField] private ScrollRect m_scrollRect;
    [SerializeField] private Transform m_textRoot;
    [SerializeField] private float m_textWidth = 150f;
    [SerializeField] private float m_textSize = 12f;
    
    [Header("Event")] 
    [SerializeField] private NpcEvent m_onNpcSelected;
    [SerializeField] private EvidenceEvent  m_sentNoteToTheoryBoardEvent;
    [SerializeField] private EventChannel m_refreshTree;
    [SerializeField] private Check m_checkIfMarked;

    [Header("Hover Component")]
    [SerializeField] private UIHoverDetector m_hoverDetector;


    [SerializeField] private Vector3 centerPosition;
    [SerializeField] private float centerScale = 1.0f;

    private DialogueNote _activeNote;
    private CancellationTokenSource _textCancellationTokenSource;
    private DynamicUIText _currentActiveText;
    
    private readonly List<IFlyweight> _spawnedFlyweights = new();
    private readonly List<ImageSelector> _arrow = new();
    private readonly List<Image> _images = new();
    
    private float _currentScale = 1f;
    private float _targetScale = 1f;

    private void Start()
    {
       m_onNpcSelected.OnEventRaised += ShowTreeFor;
       m_refreshTree.OnEventRaised += RefreshTree;
    }

    private void Update()
    {
        if (m_hoverDetector == null || !m_hoverDetector.IsMouseHovering) return;

        HandleZoomToMouse();
        Focus();
    }
    
    private void HandleZoomToMouse()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) < 0.001f && Mathf.Approximately(_currentScale, _targetScale)) return;

        if (Mathf.Abs(scrollInput) > 0.001f)
        {
            _targetScale += scrollInput * zoomSpeed * Time.deltaTime;
            _targetScale = Mathf.Clamp(_targetScale, minScale, maxScale);
        }
        
        Vector3 mouseWorldPosBefore = GetMouseWorldPosOnCanvas();
        Vector3 mouseLocalPosBefore = m_treeRoot.InverseTransformPoint(mouseWorldPosBefore);

        _currentScale = Mathf.Lerp(_currentScale, _targetScale, Time.deltaTime * zoomSmoothing);
        m_treeRoot.localScale = new Vector3(_currentScale, _currentScale, 1f);
        
        Vector3 mouseWorldPosAfter = m_treeRoot.TransformPoint(mouseLocalPosBefore);

        Vector3 worldOffset = mouseWorldPosBefore - mouseWorldPosAfter;
        Vector3 localOffset = m_treeRoot.parent.InverseTransformVector(worldOffset); 
        
        m_treeRoot.localPosition += localOffset;
    }

    private void Focus()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ResetScrollAndScale();
        }
    }


    private void ResetScrollAndScale()
    {
        if (m_treeRoot == null) return;


        if (m_scrollRect != null)
        {
            m_scrollRect.StopMovement();
        }

      
        m_treeRoot.localPosition = new Vector3(centerPosition.x, centerPosition.y, 0f);

    
        _targetScale = centerScale;
        _currentScale = centerScale;
        m_treeRoot.localScale = new Vector3(centerScale, centerScale, 1f);

    }

    private Vector3 GetMouseWorldPosOnCanvas()
    {
        RectTransform parentRect = m_treeRoot.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, Input.mousePosition, Camera.main, out Vector3 worldPoint))
        {
            return worldPoint;
        }
        return m_treeRoot.position;
    }

    private void OnDestroy()
    {
        m_onNpcSelected.OnEventRaised -= ShowTreeFor;
        m_refreshTree.OnEventRaised -= RefreshTree;
    }

    private void ShowTreeFor(NpcIdentity npcIdentity)
    {
        DespawnUI();
        var npcTrees = NotebookManager.Instance.GetDialoguesFor(npcIdentity);
        if (npcTrees is { Count: > 0 })
        {
            BuildTree(npcTrees[0]).Forget();
        }
    }

    private async UniTask BuildTree(DialogueNote dialogueNote) 
    {
        if (dialogueNote == null || !dialogueNote.GetFullDialogue()) return;
        _activeNote = dialogueNote;

        Dialogue dialogueAsset = dialogueNote.GetFullDialogue();
        if (dialogueAsset.startingNode == null) return;
        
        TreeNode runtimeRoot = BuildRuntimeTreeRecursively(dialogueAsset.startingNode, null);
        if (runtimeRoot == null) return;
        
        ReingoldTilfordLayout.CalculatePositions(runtimeRoot);
        
        
        
        SpawnNodesRecursively(runtimeRoot, 0);

        await UniTask.Yield();
        
        SpawnConnectionsRecursively(runtimeRoot);
        
        ResetScrollAndScale();
    }

    private void SpawnNodesRecursively(TreeNode node, int level)
    {
        if (node == null) return;

        Vector2 localUiPos = new Vector2(node.X * baseHorizontalSpacing, -level * levelVerticalDistance);
      
        if (node.IsLocked)
        {
            Image lockedImg = Instantiate(m_lockImage, m_treeRoot);
            lockedImg.transform.localScale = Vector3.one;
            lockedImg.transform.localPosition = localUiPos;
            lockedImg.gameObject.name = $"Locked_Node_Lvl{level}";
            lockedImg.color = m_lockColor;
            
            node.RuntimeRect = lockedImg.rectTransform;
            _images.Add(lockedImg);
        }
        else
        {
            if (node.Source is DialogueNode npcNode)
            {
                string defaultName = npcNode.PreviousResponse != null 
                    ? npcNode.PreviousResponse.responseText 
                    : "Beginning";

                var fragmentEvidenceToMark = EvidenceDataBase.Instance.GetOrCreate(
                    npcNode.guid, 
                    () => new DialogueFragmentNote(defaultName, npcNode.guid, npcNode.doesItProveAnything, npcNode)
                );

                ButtonWithSubButton button = FlyweightFactory.Instance.Spawn<ButtonWithSubButton>(m_nodeButton, Vector3.zero, Quaternion.identity, m_treeRoot);
                button.RemoveAllListeners(); 
                button.SetText(fragmentEvidenceToMark.displayName);
                
                node.RuntimeRect = button.transform as RectTransform;
                
                var subButton = button.AddSubButton();
                subButton.RemoveAllListeners(); 
                
                bool isAlreadyMarked = m_checkIfMarked.Request(fragmentEvidenceToMark.guid);
                subButton.PlayAnimation(isAlreadyMarked);
                
                subButton.AddListener(() =>
                {
                    bool currentMarkedState = m_checkIfMarked.Request(fragmentEvidenceToMark.guid);
                    if (currentMarkedState)
                    {
                        m_sentNoteToTheoryBoardEvent?.Raise(fragmentEvidenceToMark);
                        subButton.PlayAnimation(false); 
                    }
                    else
                    {
                        m_sentNoteToTheoryBoardEvent?.Raise(fragmentEvidenceToMark);
                    }
                });
                
                button.transform.localPosition = localUiPos;

                int instanceId = button.gameObject.GetHashCode();
                Debug.Log($"[TreePage Debug] Lvl:{level} | Data Name:{fragmentEvidenceToMark.displayName} | IsMarked Data:{isAlreadyMarked} | UI Object Hash:{instanceId}");

                button.gameObject.name = $"Lvl{level}_{fragmentEvidenceToMark.displayName}";
                
                button.AddListener(() =>
                {
                    OnNodeButtonClicked(npcNode.dialogueText).Forget();
                });
                
                _spawnedFlyweights.Add(button);
            }
        }
        
        foreach (var child in node.Children) 
        {
            SpawnNodesRecursively(child, level + 1);
        }
    }
    
    private void SpawnConnectionsRecursively(TreeNode node)
    {
        if (node == null) return;
        if (node.IsLocked) return;
   
        Vector2 parentCenter = new Vector2(node.X * baseHorizontalSpacing, -GetNodeLevel(node) * levelVerticalDistance);
   
        float parentHalfWidth = 40f; 
        float parentHalfHeight = 20f; 
        if (node.RuntimeRect != null)
        {
            parentHalfWidth = (node.RuntimeRect.rect.width * node.RuntimeRect.localScale.x) * 0.5f;
            parentHalfHeight = (node.RuntimeRect.rect.height * node.RuntimeRect.localScale.y) * 0.5f;
        }
   
        var validChildren = node.Children
            .Where(c => c != null)
            .OrderBy(c => c.X)
            .ToList();

        int totalChildren = validChildren.Count;
        if (totalChildren == 0) return;
   
        for (int i = 0; i < totalChildren; i++)
        {
            TreeNode child = validChildren[i];
           
            Vector2 childCenter = new Vector2(child.X * baseHorizontalSpacing, -GetNodeLevel(child) * levelVerticalDistance);
   
            float childHalfWidth = 35f; 
            float childHalfHeight = 25f; 
            if (child.RuntimeRect != null)
            {
                childHalfWidth = (child.RuntimeRect.rect.width * child.RuntimeRect.localScale.x) * 0.5f;
                childHalfHeight = (child.RuntimeRect.rect.height * child.RuntimeRect.localScale.y) * 0.5f;
            }
   
            float t = 0f;
            if (totalChildren > 1)
            {
                float normalize = (float)i / (totalChildren - 1); 
                t = Mathf.Lerp(-1f, 1f, normalize); 
            }
   
            float extraYDrop = Mathf.Abs(t) * extraYDropIntensity; 
   
            Vector2 arrowStartPos = parentCenter + new Vector2(
                t * parentHalfWidth * spreadIntensity, 
                -parentHalfHeight - extraYDrop
            );
   
            Vector2 arrowEndPos = childCenter + new Vector2(
                t * childHalfWidth * spreadIntensity, 
                childHalfHeight
            );
       
            SpawnArrow(arrowStartPos, arrowEndPos, child, t);
       
            SpawnConnectionsRecursively(child);
        }
    }

    private void SpawnArrow(Vector2 parentPos, Vector2 childPos, TreeNode childNode, float tX)
    {
        Vector2 direction = childPos - parentPos;
        float distance = direction.magnitude; 
    
        if (distance < 0.1f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, angle - angleOffset);
       
        float walkableDistance = distance - (arrowPadding * 2f);

        if (walkableDistance <= 0)
        {
            Vector2 centerPos = Vector2.Lerp(parentPos, childPos, 0.5f);
            CreateSingleArrow(centerPos, arrowRotation);
            return;
        }

        int arrowCount = Mathf.Max(1, Mathf.FloorToInt(walkableDistance / m_arrowStepDistance));
 
        for (int i = 0; i < arrowCount; i++)
        {
            float t = (arrowCount == 1) ? 0.5f : (float)i / (arrowCount - 1);
        
            float actualT = Mathf.Lerp(arrowPadding / distance, 1f - (arrowPadding / distance), t);
            Vector2 arrowLocalPos = Vector2.Lerp(parentPos, childPos, actualT);

            CreateSingleArrow(arrowLocalPos, arrowRotation);
        }
    }

    private void CreateSingleArrow(Vector2 localPos, Quaternion rotation)
    {
        ImageSelector arrow = Instantiate(arrowImage, m_treeRoot);
        
        arrow.transform.localScale = Vector3.one; 
        arrow.transform.localPosition = localPos;
        arrow.transform.localRotation = rotation;
        arrow.SetRandomSprite();
        
        if (arrow.transform is RectTransform rectTrans)
        {
            var img = arrow.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.type = UnityEngine.UI.Image.Type.Simple;
        }

        _arrow.Add(arrow);
    }

    private int GetNodeLevel(TreeNode node)
    {
        int level = 0;
        TreeNode current = node;
        while (current.Parent != null)
        {
            level++;
            current = current.Parent;
        }
        return level;
    }

    private void RefreshTree()
    {
        if (_activeNote == null) return;
        DespawnUI();
        BuildTree(_activeNote).Forget();
    }

    private void DespawnUI()
    {
        foreach (var flyweight in _spawnedFlyweights)
        {
            FlyweightFactory.Instance.Return(flyweight);
        }
        _spawnedFlyweights.Clear();

        foreach (var arrow in _arrow)
        {
            Destroy(arrow.gameObject);
        }
        _arrow.Clear();

        foreach (var image in _images)
        {
            Destroy(image.gameObject);
        }
        _images.Clear();
    }
    
    private async UniTask OnNodeButtonClicked(string contentText)
    {
        CancelAndDisposeToken();
        _textCancellationTokenSource = new CancellationTokenSource();
        
        if (_currentActiveText != null)
        {
            FlyweightFactory.Instance.Return(_currentActiveText);
            _currentActiveText = null;
        }
 
        await PlayText(contentText, _textCancellationTokenSource.Token, sizeOverride: m_textSize);
    }

    private void CancelAndDisposeToken()
    {
        if (_textCancellationTokenSource == null) return;
        _textCancellationTokenSource.Cancel();
        _textCancellationTokenSource.Dispose();
        _textCancellationTokenSource = null;
    }

    private async UniTask PlayText(string text, CancellationToken token, Transform parent = null, float sizeOverride = 0) 
    {
        if (token.IsCancellationRequested) return;
        if (text == null) return;
        
        _currentActiveText = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dynamicTextSetting, 
            Vector3.zero, 
            Quaternion.identity, 
            parent != null ? parent : m_textRoot
        );
        
        _currentActiveText.SetText(
            text, 
            !Mathf.Approximately(sizeOverride, 0) ? sizeOverride : m_dynamicTextSetting.size, 
            m_dynamicTextSetting.color, 
            m_textWidth, 
            true
        );
        _currentActiveText.ToLast();

        await UniTask.NextFrame(token);
        try
        {
            await _currentActiveText.PlayTypeWriterEffect(externalToken: token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private TreeNode BuildRuntimeTreeRecursively(DialogueNode configNode, TreeNode parentRtNode)
    {
        if (configNode == null) return null;

        bool isUnlocked = _activeNote.IsNodeUnlocked(configNode.guid);
        TreeNode rtNode = new TreeNode(configNode, isLocked: !isUnlocked)
        {
            Parent = parentRtNode
        };

        if (!isUnlocked || configNode.responses == null) return rtNode;

        foreach (var response in configNode.responses.Where(response => response.nextNode != null))
        {
            response.nextNode.PreviousResponse = response;
            TreeNode child = BuildRuntimeTreeRecursively(response.nextNode, rtNode);
            if (child == null) continue;

            child.Number = rtNode.Children.Count;
            rtNode.Children.Add(child);
        }

        return rtNode;
    }
}