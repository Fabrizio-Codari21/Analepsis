using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;

public sealed class DialogueGraphView : GraphView
{
    private EditorWindow editorWindow; // 作为哪个window 的 内容
    
    private Dictionary<DialogueNode, NpcResponse> dialogueNodeMap = new();
    private Dictionary<DialogueResponse, PlayerResponse> responseNodeMap = new();
    private Dictionary<StickyNoteData, StickyNote>  stickyNoteDataMap = new();
    
    private DialogueSearchWindow searchWindow;
    private ConditionSearchWindow _sharedConditionSearchWindow;
    private AltDialogueSearchWindow _sharedDialogueSearchWindow;
    private bool isLoadingGraph;
    
    private Dialogue currentDialogue;
    public Dialogue CurrentDialogue => currentDialogue;
    public DialogueGraphView(EditorWindow window)
    {
        editorWindow = window; // 当前的 window 设定值
        style.flexGrow = 1; // 将 空余的地方 全部用 这个 graph view 填满
        
        this.AddManipulator(new ContentDragger()); // 加入拖拽 
         
        this.AddManipulator(new SelectionDragger()); // 加入 框选
        
        this.AddManipulator(new RectangleSelector()); // 绘制 框选 框
        
        SetupZoom(ContentZoomer.DefaultMinScale, 3.0f); //（ min 0.05 -> 2.0） 加入 zoom
        
        
        
        GridBackground grid = new GridBackground(); // 网格背景
        Insert(0, grid); // 到最底下 重点是insert 到第一个 物品
        grid.StretchToParentSize(); // 拉伸
        grid.SendToBack();  // 确保到最底下

        graphViewChanged = OnGraphViewChanged;
        focusable = true;
        
        
        RegisterCallback<KeyDownEvent>(evt =>  
        {
            if (evt.keyCode != KeyCode.Delete && evt.keyCode != KeyCode.Backspace) return;
            DeleteSelectionCallback("Delete", AskUser.DontAskUser);
            evt.StopPropagation();
        });
        
       
        
    }

    // 每次 做出改动后 的变化
    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (isLoadingGraph) return change; // 目前还在加载阶段， 不是改变

        if (change.edgesToCreate != null)
        {
            foreach (Edge edge in change.edgesToCreate) // 在所有拉出来的线中 
            {
                // 如果起点 输出口是 玩家的回答，结尾输入是 npc 的对话
                if (edge.output.node is PlayerResponse responseNode && edge.input.node is NpcResponse dialogueNode) 
                {
                    responseNode.ResponseData.nextNode = dialogueNode.NodeData; // 回答的后的回话 就是 输入的这个 对话
                }
            }
        }
    
        if (change.elementsToRemove == null) return change; // 
        
            foreach (GraphElement element in change.elementsToRemove)
            {
                switch (element)
                {
                    case Edge edge:
                    {
                        edge.input?.Disconnect(edge);
                        edge.output?.Disconnect(edge);

                        if (edge.output is { node: PlayerResponse responseNode }) responseNode.ResponseData.nextNode = null;
                        
                        break;
                    }
                    case PlayerResponse responseGraphNode:
                        RemoveResponseNodeData(responseGraphNode);
                        break;
                    case NpcResponse dialogueGraphNode:
                        RemoveDialogueNodeData(dialogueGraphNode);
                        break;
                    case DialogueStickyNote note:
                        RemoveStickyNoteData(note);
                        break;
                       
                }
            }
           
            
        

        return change;
    }
    
    // 加载 dialogue， 先清除所有的，然后在重新加载
    public void LoadDialogue(Dialogue dialogue)
    {
        
        currentDialogue = dialogue;   
        // 开始加载
        isLoadingGraph = true;
        // 清除
        DeleteElements(graphElements.ToList());
        dialogueNodeMap.Clear();
        responseNodeMap.Clear();
        stickyNoteDataMap.Clear();

        if (dialogue.startingNode == null) // 如果没有任何一个起始点，代表没有任何一个，停止加载
        {
            isLoadingGraph = false;
            return;
        }

        HashSet<DialogueNode> visited = new(); // 创建一个 hash set ，储存已经 遍历过的 node，这里的node 将会是 游戏中的 data 了
        CreateDialogueTree(dialogue.startingNode, new Vector2(300, 200), visited); // 从开始的 node，在 x 300 y200 的位置
        
        foreach (var noteData in currentDialogue.StickyNotes)
        {
            LoadStickyNote(noteData);
        }
        isLoadingGraph = false;
    }
    
    private void CreateDialogueTree(DialogueNode nodeData, Vector2 position, HashSet<DialogueNode> visited)
    {
        if (nodeData == null || !visited.Add(nodeData)) return; // 如果已经重复了的话，就不要添加，也不用继续了

        bool isRoot = visited.Count == 1; // 如果只有一个 代表是 第一个 node ，也就是 root
        NpcResponse npcResponseNode = CreateNpcResponseNode(position, isRoot ,nodeData); // 创建 npc 的对话

        float yOffset = 0f;

        foreach (DialogueResponse response in nodeData.responses)  // 在 npc 讲完话后 可以对 npc 的回话
        {
            Vector2 responsePosition = position + new Vector2(350, yOffset); 

            PlayerResponse playerResponseNode = CreatePlayerResponseNode(response, responsePosition); // 创建 玩家的对话节点
            
            Edge npcToPlayerEdge = npcResponseNode.OutputPort.ConnectTo(playerResponseNode.InputPort); // 把 这个 npc 的输出端 连接到这个 对话的 输入端，线
            AddElement(npcToPlayerEdge);

            if (response.nextNode != null)
            {
                Vector2 nextDialoguePosition = responsePosition + new Vector2(350, 0);

                CreateDialogueTree(response.nextNode, nextDialoguePosition, visited);

                NpcResponse nextDialogueNode = GetDialogueGraphNode(response.nextNode);

                if (nextDialogueNode is { InputPort: not null })
                {
                    Edge responseToDialogue = playerResponseNode.OutputPort.ConnectTo(nextDialogueNode.InputPort);
                    AddElement(responseToDialogue);
                }
            }
            yOffset += 250f;
        }
    }
    public NpcResponse CreateNpcResponseNode(Vector2 position, bool isRoot = false, DialogueNode nodeData = null)
    {
        
        // 游戏数据的创建
        nodeData ??= new DialogueNode(); // 如果没有就创建一个新的 空的dialogue node 
        nodeData.isRootNode = isRoot;  // 告诉 游戏中的 这个 node 是否为 第一个 node

        
        if (currentDialogue != null) 
        {
            // all node ，这个dialogue 中所有的node，如果没有的话 就添加 
            bool exists = currentDialogue.allNodes.Any(n => n.guid == nodeData.guid); 
            if (!exists)
            {
                currentDialogue.allNodes.Add(nodeData);
                EditorUtility.SetDirty(currentDialogue); 
            } }
        if (nodeData.editorPosition != Vector2.zero) // 记录这个 node 在 编辑器的位置在哪里，并更改
        {
            position = nodeData.editorPosition;
        }
        
        // 编辑器 数据的创建， 先根据 游戏数据 创建一个容器
        NpcResponse node = new NpcResponse(nodeData, this);

        // 设定 这个 容器的位置在哪里，以及他的大小
        node.SetPosition(new Rect(position, new Vector2(250, 150)));

        AddElement(node); //这个 容器 加入到graph view 中，这个函数为 graph view 自带的，这样这个元素才能被鼠标 交互

        dialogueNodeMap[nodeData] = node;
        return node;
    }
    
    public PlayerResponse CreatePlayerResponseNode(DialogueResponse response, Vector2 position)
    {
        if (response.editorPosition != Vector2.zero)
        {
            position = response.editorPosition;
        }

        PlayerResponse responseNode = new PlayerResponse(response,this);

        responseNode.SetPosition(new Rect(position, new Vector2(250, 150)));

        AddElement(responseNode);

        responseNodeMap[response] = responseNode;
        
        return responseNode;
    }
    public void OpenSearchWindow(Port port, Vector2 position)
    {
        searchWindow = ScriptableObject.CreateInstance<DialogueSearchWindow>();
        searchWindow.Initialize(this, editorWindow);
        searchWindow.SetContext(port, position);
        SearchWindow.Open(new SearchWindowContext(position), searchWindow);
    }
    public void OpenConditionSearchWindow(PlayerResponse node, Vector2 mousePos)
    {
        if (_sharedConditionSearchWindow == null) _sharedConditionSearchWindow = ScriptableObject.CreateInstance<ConditionSearchWindow>();
        _sharedConditionSearchWindow.Init(node);
        SearchWindow.Open(new SearchWindowContext(mousePos), _sharedConditionSearchWindow);
    }
    public void OpenAltDialogueSearchWindow(NpcResponse node, Vector2 mousePos)
    {
        if (_sharedDialogueSearchWindow == null) _sharedDialogueSearchWindow = ScriptableObject.CreateInstance<AltDialogueSearchWindow>();

        _sharedDialogueSearchWindow.Init(node);
        SearchWindow.Open(new SearchWindowContext(mousePos), _sharedDialogueSearchWindow);
    }

    private void CreateStickyNote(Vector2 position)
    {
        if (currentDialogue == null) return;

       
        StickyNoteData noteData = new StickyNoteData
        {
            Title = "Title",
            Content = "Contents",
            Position = position
        };

        currentDialogue.StickyNotes.Add(noteData);
        EditorUtility.SetDirty(currentDialogue);
        
        DialogueStickyNote note = new DialogueStickyNote(noteData, this);
        AddElement(note);
        stickyNoteDataMap[noteData] = note;
    }

    private void LoadStickyNote(StickyNoteData noteData)
    {
        DialogueStickyNote note = new DialogueStickyNote(noteData, this);
        AddElement(note);
        stickyNoteDataMap[noteData] = note;
    }
    
    private void RemoveStickyNoteData(DialogueStickyNote note)
    {
        if (note.TargetData == null) return;

        if (currentDialogue != null && currentDialogue.StickyNotes.Contains(note.TargetData))
        {
            currentDialogue.StickyNotes.Remove(note.TargetData);
            EditorUtility.SetDirty(currentDialogue);
        }

        stickyNoteDataMap.Remove(note.TargetData);
    }
    private void DeleteSelectionCallback(string operationName, AskUser askUser)
    {
        List<GraphElement> elementsToDelete = new();

        foreach (ISelectable selectable in selection)
        {
            if (selectable is NpcResponse dialogueNode)
            {
                if (dialogueNode.NodeData.isRootNode) continue;
            }
            
            if (selectable is not GraphElement element) continue;
            
            elementsToDelete.Add(element);

            if (element is not Node node) continue;
            
            foreach (Port port in node.inputContainer.Children().OfType<Port>())
            {
                foreach (Edge edge in port.connections)
                {
                    if (!elementsToDelete.Contains(edge)) elementsToDelete.Add(edge);
                }
            }
            foreach (Port port in node.outputContainer.Children().OfType<Port>())
            {
                foreach (Edge edge in port.connections)
                {
                    if (!elementsToDelete.Contains(edge)) elementsToDelete.Add(edge);
                }
            }
        }
        DeleteElements(elementsToDelete);
    }

   

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 mousePosition = evt.localMousePosition;

        evt.menu.AppendAction("Create Dialogue Node", action =>
        {
            CreateNpcResponseNode(mousePosition);
        });
        
        evt.menu.AppendSeparator();
        evt.menu.AppendAction("Sticky note", action =>
        {
            CreateStickyNote(mousePosition);
        });
    }
    
    
    
    
    public NpcResponse GetDialogueGraphNode(DialogueNode nodeData)
    {
        return dialogueNodeMap.GetValueOrDefault(nodeData);
    }
 
   
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new();

        ports.ForEach(port =>
        {
            if (startPort == port)
                return;

            if (startPort.node == port.node)
                return;

            if (startPort.direction == port.direction)
                return;

            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }
    private void RemoveResponseNodeData(PlayerResponse response)
    {
        DialogueResponse responseData = response.ResponseData;

        foreach (var pair in dialogueNodeMap)
        {
            DialogueNode dialogueNode = pair.Key;

            if (dialogueNode.responses != null &&
                dialogueNode.responses.Contains(responseData))
            {
                dialogueNode.responses.Remove(responseData);
                break;
            }
        }
        responseNodeMap.Remove(responseData);
       
    }
    
    private void RemoveDialogueNodeData(NpcResponse npcResponse)
    {
        DialogueNode nodeData = npcResponse.NodeData;

        if (currentDialogue != null && currentDialogue.allNodes.Contains(nodeData))
        {
            currentDialogue.allNodes.Remove(nodeData);
            EditorUtility.SetDirty(currentDialogue);
        }
        foreach (var pair in responseNodeMap)
        {
            DialogueResponse response = pair.Key;

            if (response.nextNode == nodeData)
            {
                response.nextNode = null;
            }
        }
        if (nodeData.responses != null)
        {
            List<DialogueResponse> responsesToRemove = new(nodeData.responses);

            foreach (DialogueResponse response in responsesToRemove)
            {
                if (responseNodeMap.TryGetValue(response, out var responseGraphNode))
                {
                    RemoveElement(responseGraphNode);
                }

                responseNodeMap.Remove(response);
            }

            nodeData.responses.Clear();
        }

        dialogueNodeMap.Remove(nodeData);
    }
    
  
}

public class DialogueStickyNote : StickyNote
{
    public StickyNoteData TargetData { get; private set; }
    private readonly DialogueGraphView graphView;
    public DialogueStickyNote(StickyNoteData data, DialogueGraphView graphView)
    {
        this.TargetData = data;
        this.graphView = graphView;

        title = data.Title;
        contents = data.Content;
        SetPosition(new Rect(data.Position, new Vector2(250, 150)));
        capabilities |= Capabilities.Droppable | Capabilities.Movable | Capabilities.Deletable;

        // 监听标题和正文文本框的修改事件
        var titleLabel = this.Q<TextField>("title-field");
        titleLabel?.RegisterValueChangedCallback(evt =>
        {
            TargetData.Title = evt.newValue;
            MarkDirty();
        });

        var contentLabel = this.Q<TextField>("contents-field");
        contentLabel?.RegisterValueChangedCallback(evt =>
        {
            TargetData.Content = evt.newValue;
            MarkDirty();
        });
    }

    
    public sealed override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        if (TargetData != null)
        {
            TargetData.Position = newPos.position;
            MarkDirty();
        }
    }

    private void MarkDirty()
    {
        if (graphView.CurrentDialogue != null)
        {
            EditorUtility.SetDirty(graphView.CurrentDialogue);
        }
    }
}