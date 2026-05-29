using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TreePage : NotebookPage
{
    [SerializeField] private Transform m_treeRoot;
  
    [Header("Tree Layout Geometry (Pure World Units)")]
    [SerializeField] private float levelVerticalDistance = 60.0f;
    [SerializeField] private float baseHorizontalSpacing = 80.0f; 
    
    [Header("UI REFERENCES")]
    [Space(5)]
    [Header("Button")]
    [SerializeField] private ButtonSetting m_nodeButton;
    
    [Header("Arrow")]
    [SerializeField] private ImageSelector arrowImage;
    [SerializeField] private float angleOffset = 30f;
    [Header("Lock Image")]
    [SerializeField] private Color m_lockColor = new (0.4f, 0, 0.1f, 0.1f);
    [SerializeField] private Image m_lockImage;
    
    [Header("Text")]
    [SerializeField] private DynamicTextSetting m_dynamicTextSetting;
    [SerializeField] private ScrollRect m_scrollRect;
    [SerializeField] private Transform m_textRoot;
    [SerializeField] private float m_textWidth = 150f;
    [SerializeField] private float m_textSize = 12f;
    
    private CancellationTokenSource _textCancellationTokenSource;
    private DynamicUIText _currentActiveText;
    
    private async UniTask OnNodeButtonClicked(string contentText)
    {
       
        CancelAndDisposeToken();
        _textCancellationTokenSource = new CancellationTokenSource();
        
        if (_currentActiveText != null)
        {
            FlyweightFactory.Instance.Return(_currentActiveText);
            _currentActiveText = null;
        }
 
        await PlayText(contentText, _textCancellationTokenSource.Token,sizeOverride:m_textSize);
    }
    private void CancelAndDisposeToken()
    {
        if (_textCancellationTokenSource != null)
        {
            _textCancellationTokenSource.Cancel();
            _textCancellationTokenSource.Dispose();
            _textCancellationTokenSource = null;
        }
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
    private DialogueNote _activeNote;
    [SerializeField] private NpcEvent m_onNpcSelected;

    private void Start()
    {
        if (m_onNpcSelected != null) m_onNpcSelected.OnEventRaised += ShowTreeFor;
    }

    private void OnDestroy()
    {
        if (m_onNpcSelected != null) m_onNpcSelected.OnEventRaised -= ShowTreeFor;
    }

    
    
    private void ShowTreeFor(NpcIdentity npcIdentity)
    {   
        foreach (Transform child in m_treeRoot)
        {
            Destroy(child.gameObject); 
        }

        var npcTrees = NotebookManager.Instance.GetDialoguesFor(npcIdentity);
        if (npcTrees is { Count: > 0 })
        {
            BuildTree(npcTrees[0]).Forget();
        }
    }


    private async UniTask BuildTree(DialogueNote dialogueNote) 
    {
        if (dialogueNote == null || dialogueNote.GetFullDialogue() == null) return;
        _activeNote = dialogueNote;

        Dialogue dialogueAsset = dialogueNote.GetFullDialogue();

        if (dialogueAsset.startingNode == null) return;
        
        TreeNode runtimeRoot = BuildRuntimeTreeRecursively(dialogueAsset.startingNode, null);

        if (runtimeRoot == null) return;
        
        ReingoldTilfordLayout.CalculatePositions(runtimeRoot);
        
        SpawnNodesRecursively(runtimeRoot, 0);

        await UniTask.Yield();
        
        SpawnConnectionsRecursively(runtimeRoot);
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
        }
        else
        {
            if (node.Source is DialogueNode npcNode)
            {
                string nodeName = npcNode.PreviousResponse != null ? npcNode.PreviousResponse.responseText : "Beginning";
                Debug.Log("El nombre de node es  "  +  nodeName);
                ButtonFactoryObject button = SpawnClueButton(nodeName, m_treeRoot);
                button.transform.localPosition = localUiPos;
                button.gameObject.name = $"Unlocked_Node_Lvl{level}";
                
                
                button.AddListener(() =>
                {
                    OnNodeButtonClicked(npcNode.dialogueText).Forget();
                });
                
                button.AddListenerToSub(() =>
                {
                    
                });
            }
        }
        
        foreach (var child in node.Children) SpawnNodesRecursively(child, level + 1);
        
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
    
   
    private void SpawnConnectionsRecursively(TreeNode node)
    {
        if (node == null) return;

        Vector2 parentPos = new Vector2(node.X * baseHorizontalSpacing, -GetNodeLevel(node) * levelVerticalDistance);

        foreach (var child in node.Children)
        {
            Vector2 childPos = new Vector2(child.X * baseHorizontalSpacing, -GetNodeLevel(child) * levelVerticalDistance);
            
            SpawnArrow(parentPos, childPos, child);
            SpawnConnectionsRecursively(child);
        }
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

    
    private void SpawnArrow(Vector2 parentPos, Vector2 childPos, TreeNode childNode)
    {
        Vector3 arrowLocalPos = Vector3.Lerp(parentPos, childPos, 0.5f);
        ImageSelector arrow = Instantiate(arrowImage, m_treeRoot);
        arrow.transform.localScale = Vector3.one;
        arrow.transform.localPosition = arrowLocalPos;
        arrow.SetRandomSprite();

        Vector3 direction = childPos - parentPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.localRotation = Quaternion.Euler(0f, 0f, angle - angleOffset);
    }

    private ButtonFactoryObject SpawnClueButton(string displayName, Transform treeRoot)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(m_nodeButton, Vector3.zero, Quaternion.identity, treeRoot);
        button.SetText(displayName);
        button.SetInteractable(true);
        button.MoveToLast();
        
        return button;
    }
}