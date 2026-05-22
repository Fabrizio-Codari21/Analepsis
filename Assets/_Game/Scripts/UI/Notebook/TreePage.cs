using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TreePage : NotebookPage
{
   
    [SerializeField] private Transform m_treeRoot;
  
    
    [Header("Tree Layout Geometry (Pure World Units)")]
    [SerializeField] private float levelVerticalDistance = 40.0f;
    [SerializeField] private float baseHorizontalSpacing = 30.0f;
    
    
    [Header("UI REFERENCES")]
    [Header("Tree")]
    [SerializeField] private ButtonSetting m_nodeButton;
    [SerializeField] private ImageSelector arrowImage;
    [SerializeField] private float angleOffset = 30f;
    [SerializeField] private Color m_lockColor = new (0.4f, 0, 0.1f, 0.1f);
    [SerializeField] private Image m_lockImage;


    
    private Dialogue _currentDialogue;
    private List<INode> _unlockedDialogue;

    private Dictionary<int, List<DialogueNode>> _levelMap = new();
    private Dictionary<DialogueNode, Vector2> _nodePositions = new();
    
    public void ShowTreeFor(NpcIdentity npcIdentity)
    {   
        
    }
    public async UniTask BuildTree(DialogueNote dialogue)
    {
        _currentDialogue = dialogue.GetFullDialogue();
        _unlockedDialogue = dialogue.GetUnlockedDialogue();

        _levelMap.Clear();
        _nodePositions.Clear();

        DialogueNode root = _currentDialogue.startingNode;

        CollectLevels(root);

        await LayoutLevels();
    }
    
    private void CollectLevels(DialogueNode root)
    {
        Queue<(DialogueNode node, int level)> queue = new();
        
        queue.Enqueue((root, 0));

        while (queue.Count > 0)
        {
            var (node, level) = queue.Dequeue();

            if (!_levelMap.ContainsKey(level)) _levelMap[level] = new List<DialogueNode>();
            

            if (!_levelMap[level].Contains(node)) _levelMap[level].Add(node);
            

            foreach (var response in node.responses.Where(response => response.nextNode != null))
            {
                response.nextNode.PreviousResponse = response;
                queue.Enqueue((response.nextNode, level + 1));
            }
        }
    }
    
    private async UniTask LayoutLevels()
    {
        foreach (var (level, nodes) in _levelMap)
        {
            int count = nodes.Count;

            float totalWidth = (count - 1) * baseHorizontalSpacing;

            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                DialogueNode node = nodes[i];

                Vector2 pos = new(startX + i * baseHorizontalSpacing, -level * levelVerticalDistance);

                _nodePositions[node] = pos;

                SpawnNode(node, pos, level);
            }
        }

        await UniTask.Yield();

        SpawnAllConnections();
    }
    
    private void SpawnAllConnections()
    {
        foreach (var pair in _levelMap)
        {
            foreach (var node in pair.Value)
            {
                Vector2 parentPos = _nodePositions[node];

                foreach (var response in node.responses.Where(response => response.nextNode != null).Where(response => _nodePositions.ContainsKey(response.nextNode)))
                {
                    response.nextNode.PreviousResponse = response;
                    Vector2 childPos = _nodePositions[response.nextNode];
                    SpawnArrow(parentPos, childPos, response.nextNode);
                }
            }
        }
    }

    
    private void SpawnArrow(Vector2 parentPos, Vector2 childPos, DialogueNode node)
    {
        Vector3 arrowLocalPos = Vector3.Lerp(parentPos, childPos, 0.5f);

        ImageSelector arrow = Instantiate(arrowImage, m_treeRoot);

        arrow.transform.localScale = Vector3.one;

        arrow.transform.localPosition = arrowLocalPos;

        arrow.SetRandomSprite();

        Vector3 direction = childPos - parentPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        arrow.transform.localRotation = Quaternion.Euler(0f, 0f, angle - angleOffset);

        if (node.PreviousResponse != null && !node.PreviousResponse.IsAvailable()) arrow.baseImage.color = m_lockColor;
        
    }
    
    private void SpawnNode(DialogueNode node, Vector2 position, int level)
    {
        LogNote note = new(node.PreviousResponse != null ? node.PreviousResponse.responseText : "Beginning", new List<string> { node.dialogueText }, new List<string> { node.dialogueText }, new(_currentDialogue, new() { node.doesItProveAnything }));

        if (node.PreviousResponse != null && !node.PreviousResponse.IsAvailable())
        {
            Image locked = Instantiate(m_lockImage, m_treeRoot);

            locked.transform.localScale = Vector3.one;

            locked.transform.localPosition = position;

            locked.transform.localRotation = Quaternion.identity;

            locked.GetComponentInChildren<Image>().color = m_lockColor;

            return;
        }

        bool unread = !_unlockedDialogue.Contains(node);

        ButtonFactoryObject button = SpawnClueButton(note, m_treeRoot, unread);
        
        button.transform.localPosition = position;
        button.gameObject.name = $"Button_{level}";
    }

    private ButtonFactoryObject SpawnClueButton(Note note, Transform treeRoot, bool unread)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            m_nodeButton,
            Vector3.zero,
            Quaternion.identity,
            treeRoot
        );
        button.SetText(note.displayName);
        button.SetInteractable(true);
        button.MoveToLast();
  
        return button;
    }
}