using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView graphView;
    private EditorWindow window;

    private Port draggedPort;
    private Vector2 mousePosition;

    public void Initialize(DialogueGraphView graphView, EditorWindow window)
    {
        this.graphView = graphView;
        this.window = window;
    }

    public void SetContext(Port draggedPort, Vector2 mousePosition)
    {
        this.draggedPort = draggedPort;
        this.mousePosition = mousePosition;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
        };

        if (draggedPort.direction == Direction.Output)
        {
            if (draggedPort.node is DialogueGraphNode)
            {
                tree.Add(new SearchTreeEntry(new GUIContent("Response Node"))
                {
                    level = 1,
                    userData = "ResponseNode"
                });
            }
            else if (draggedPort.node is DialogueResponseGraphNode)
            {
                tree.Add(new SearchTreeEntry(new GUIContent("Dialogue Node"))
                {
                    level = 1,
                    userData = "DialogueNode"
                });
            }
        }

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        Vector2 worldPosition = window.rootVisualElement.ChangeCoordinatesTo(
            window.rootVisualElement.parent,
            mousePosition - window.position.position
        );

        Vector2 localPosition = graphView.contentViewContainer.WorldToLocal(worldPosition);

        switch ((string)entry.userData)
        {
            case "DialogueNode":
            {
                DialogueNode dialogueData = new DialogueNode
                {
                    dialogueText = "New Dialogue"
                };

                DialogueGraphNode dialogueNode = graphView.CreateNode(localPosition,nodeData: dialogueData);

                if (draggedPort.node is DialogueResponseGraphNode responseNode)
                {
                    Edge edge = responseNode.OutputPort.ConnectTo(dialogueNode.InputPort);
                    graphView.AddElement(edge);

                    responseNode.ResponseData.nextNode = dialogueData;
                }

                break;
            }

            case "ResponseNode":
            {
                DialogueResponse responseData = new DialogueResponse
                {
                    responseText = "New Response"
                };

                DialogueResponseGraphNode responseNode =
                    graphView.CreateResponseNode(responseData, localPosition);

                if (draggedPort.node is DialogueGraphNode dialogueNode)
                {
                    dialogueNode.NodeData.responses.Add(responseData);

                    Edge edge = dialogueNode.OutputPort.ConnectTo(responseNode.InputPort);
                    graphView.AddElement(edge);
                }

                break;
            }
        }

        return true;
    }
}

public class ConditionSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueResponseGraphNode _node;

    public void Init(DialogueResponseGraphNode node)
    {
        _node = node;
    }
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Select Condition Type"), 0),
            new SearchTreeEntry(new GUIContent("Dialogue Node Condition"))
            {
                level = 1,
                userData = typeof(DialogueNodeCondition)
            },
        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        if (entry.userData is not System.Type type) return false;
        var newCondition = (DialogueCondition)System.Activator.CreateInstance(type);
        _node.ResponseData.m_conditions.Add(newCondition);
        _node.GenerateConditionUI(); 
        return true;
    }
}