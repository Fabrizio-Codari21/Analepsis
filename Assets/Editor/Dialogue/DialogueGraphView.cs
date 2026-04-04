using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public sealed class DialogueGraphView : GraphView
{
    private Dictionary<DialogueNode, DialogueGraphNode> dialogueNodeMap = new();
    private Dictionary<DialogueResponse, DialogueResponseGraphNode> responseNodeMap = new();
    
    private DialogueSearchWindow searchWindow;
    private EditorWindow editorWindow;
    private bool isLoadingGraph;
    public DialogueGraphView(EditorWindow window)
    {
        editorWindow = window;
        style.flexGrow = 1;
        
        this.AddManipulator(new ContentDragger());
        
        this.AddManipulator(new SelectionDragger());
        
        this.AddManipulator(new RectangleSelector());
        
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        
        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        grid.SendToBack();
        
        searchWindow = ScriptableObject.CreateInstance<DialogueSearchWindow>();
        searchWindow.Initialize(this, editorWindow);
        
        graphViewChanged = OnGraphViewChanged;
        deleteSelection = DeleteSelectionCallback;
        focusable = true;
        this.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode != KeyCode.Delete && evt.keyCode != KeyCode.Backspace) return;
            DeleteSelectionCallback("Delete", AskUser.DontAskUser);
            evt.StopPropagation();
        });
    }
    public void OpenSearchWindow(Port port, Vector2 position)
    {
        searchWindow.SetContext(port, position);
        SearchWindow.Open(new SearchWindowContext(position), searchWindow);
    }
    private void DeleteSelectionCallback(string operationName, AskUser askUser)
    {
        List<GraphElement> elementsToDelete = new();

        foreach (ISelectable selectable in selection)
        {
            if (selectable is DialogueGraphNode dialogueNode)
            {
                if (dialogueNode.NodeData.isRootNode)
                    continue;
            }
            if (selectable is not GraphElement element) continue;
            elementsToDelete.Add(element);

            if (element is not Node node) continue;
            foreach (Port port in node.inputContainer.Children().OfType<Port>())
            {
                foreach (Edge edge in port.connections)
                {
                    if (!elementsToDelete.Contains(edge))
                    {
                        elementsToDelete.Add(edge);
                    }
                }
            }

            foreach (Port port in node.outputContainer.Children().OfType<Port>())
            {
                foreach (Edge edge in port.connections)
                {
                    if (!elementsToDelete.Contains(edge))
                    {
                        elementsToDelete.Add(edge);
                    }
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
            CreateNode(mousePosition);
        });
    }
    
    public DialogueGraphNode CreateNode(Vector2 position, DialogueNode nodeData = null)
    {
        nodeData ??= new DialogueNode();

        if (nodeData.editorPosition != Vector2.zero)
        {
            position = nodeData.editorPosition;
        }
        DialogueGraphNode node = new DialogueGraphNode(nodeData, this);

        node.SetPosition(new Rect(position, new Vector2(250, 150)));

        AddElement(node);

        dialogueNodeMap[nodeData] = node;

        return node;
    }
    public DialogueResponseGraphNode CreateResponseNode(DialogueResponse response, Vector2 position)
    {
        if (response.editorPosition != Vector2.zero)
        {
            position = response.editorPosition;
        }

        DialogueResponseGraphNode responseNode = new DialogueResponseGraphNode(response,this);

        responseNode.SetPosition(new Rect(position, new Vector2(250, 150)));

        AddElement(responseNode);

        responseNodeMap[response] = responseNode;

        return responseNode;
    }
    
    public DialogueGraphNode GetDialogueGraphNode(DialogueNode nodeData)
    {
        return dialogueNodeMap.GetValueOrDefault(nodeData);
    }
 
    private void CreateDialogueTree(DialogueNode nodeData, Vector2 position, HashSet<DialogueNode> visited)
    {
        if (nodeData == null || visited.Contains(nodeData)) return;

        visited.Add(nodeData);

        DialogueGraphNode dialogueNode = CreateNode(position, nodeData);

        float yOffset = 0f;

        foreach (DialogueResponse response in nodeData.responses)
        {
            Vector2 responsePosition = position + new Vector2(350, yOffset);

            DialogueResponseGraphNode responseNode =
                CreateResponseNode(response, responsePosition);

            Edge dialogueToResponse = dialogueNode.OutputPort.ConnectTo(responseNode.InputPort);
            AddElement(dialogueToResponse);

            if (response.nextNode != null)
            {
                Vector2 nextDialoguePosition = responsePosition + new Vector2(350, 0);

                CreateDialogueTree(response.nextNode, nextDialoguePosition, visited);

                DialogueGraphNode nextDialogueNode = GetDialogueGraphNode(response.nextNode);

                if (nextDialogueNode != null && nextDialogueNode.InputPort != null)
                {
                    Edge responseToDialogue = responseNode.OutputPort.ConnectTo(nextDialogueNode.InputPort);
                    AddElement(responseToDialogue);
                }
            }

            yOffset += 250f;
        }
    }
    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (isLoadingGraph) return change;
        if (change.edgesToCreate != null)
        {
            foreach (Edge edge in change.edgesToCreate)
            {
                if (edge.output.node is DialogueResponseGraphNode responseNode &&
                    edge.input.node is DialogueGraphNode dialogueNode)
                {
                    responseNode.ResponseData.nextNode = dialogueNode.NodeData;
                }
            }
        }

        if (change.elementsToRemove != null)
        {
            foreach (GraphElement element in change.elementsToRemove)
            {
                switch (element)
                {
                    case Edge edge:
                    {
                        edge.input?.Disconnect(edge);
                        edge.output?.Disconnect(edge);

                        if (edge.output is { node: DialogueResponseGraphNode responseNode })
                        {
                            responseNode.ResponseData.nextNode = null;
                        }

                        break;
                    }
                    case DialogueResponseGraphNode responseGraphNode:
                        RemoveResponseNodeData(responseGraphNode);
                        break;
                    case DialogueGraphNode dialogueGraphNode:
                        RemoveDialogueNodeData(dialogueGraphNode);
                        break;
                }
            }
        }

        return change;
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
    private void RemoveResponseNodeData(DialogueResponseGraphNode responseGraphNode)
    {
        DialogueResponse responseData = responseGraphNode.ResponseData;

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
    
    private void RemoveDialogueNodeData(DialogueGraphNode dialogueGraphNode)
    {
        DialogueNode nodeData = dialogueGraphNode.NodeData;
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
    
    public void LoadDialogue(Dialogue dialogue)
    {
        isLoadingGraph = true;
        DeleteElements(graphElements.ToList());
        dialogueNodeMap.Clear();
        responseNodeMap.Clear();

        if (dialogue.startingNode == null)
        {
            isLoadingGraph = false;
            return;
        }

        HashSet<DialogueNode> visited = new();

        CreateDialogueTree(dialogue.startingNode, new Vector2(300, 200), visited);

        isLoadingGraph = false;
    }
}