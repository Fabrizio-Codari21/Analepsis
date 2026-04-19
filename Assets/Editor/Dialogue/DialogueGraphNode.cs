using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class DialogueGraphNode : Node
{
    public DialogueNode NodeData;
    public Port InputPort;
    public Port OutputPort;
    private DialogueGraphView _graphView;
    public DialogueGraphNode(DialogueNode nodeData, DialogueGraphView graphView)
    {
        if (!nodeData.isRootNode)
        {
            capabilities |= Capabilities.Deletable;
        }
        capabilities |= Capabilities.Selectable;
        capabilities |= Capabilities.Movable;
        NodeData = nodeData;
        _graphView = graphView;
        title = nodeData.isRootNode ? "Start Node" : "Dialogue Node";
        TextField textField = new TextField("Dialogue")
        {
            multiline = true,
            value = nodeData.dialogueText
        };

        textField.RegisterValueChangedCallback(evt =>
        {
            NodeData.dialogueText = evt.newValue;
            
        });

        extensionContainer.Add(textField);

        
        
        TextField tagField = new TextField("Tag")
        {
            multiline = true,
            value = nodeData.tag
        };

        tagField.RegisterValueChangedCallback(evt =>
        {
            NodeData.tag = evt.newValue;
        });

        extensionContainer.Add(tagField);

        Foldout proofFoldOut = new Foldout()
        {
            text = "Does it prove anything?",
            value = false,
        };

        PopupField<Whodunnit> proofField = 
        new PopupField<Whodunnit>("What does it prove?")
        {
            value = nodeData.doesItProveAnything,
            choices = new List<Whodunnit> 
            { 
                Whodunnit.NoProof,
                Whodunnit.Victim,
                Whodunnit.Killer,
                Whodunnit.Motive,
                Whodunnit.Weapon,
                //TheoryboardManager.Whodunnit.Place,
            }
        };

        proofField.RegisterValueChangedCallback(evt =>
        {
            NodeData.doesItProveAnything = evt.newValue;
        });

        proofFoldOut.Add(proofField);
        extensionContainer.Add(proofFoldOut);



        if (!nodeData.isRootNode)
        {
            InputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(bool)
            );
            InputPort.portName = "Input";
            InputPort.portColor = Color.cyan;
            inputContainer.Add(InputPort);
        }
        OutputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Multi,
            typeof(bool)
        );
        OutputPort.portName = "Responses";
        OutputPort.portColor = Color.yellow;
        outputContainer.Add(OutputPort);
        EdgeConnector<Edge> edgeConnector = new EdgeConnector<Edge>(new DialogueEdgeConnectorListener(_graphView));
        OutputPort.AddManipulator(edgeConnector);
        NodeData.responses ??= new List<DialogueResponse>();

        Button addResponseButton = new Button(() =>
        {
            DialogueResponse response = new DialogueResponse
            {
                responseText = "New Response"
            };
            NodeData.responses.Add(response);
            DialogueResponseGraphNode responseNode = _graphView.CreateResponseNode(response, GetPosition().position + new Vector2(300, 0));
            Edge edge = OutputPort.ConnectTo(responseNode.InputPort);
            _graphView.AddElement(edge);
        })
        {
            text = "+ Add Response"
        };

        titleButtonContainer.Add(addResponseButton);

        RefreshExpandedState();
        RefreshPorts();
    }
    
    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);

        NodeData.editorPosition = newPos.position;

        if (UnityEditor.Selection.activeObject != null)
        {
            UnityEditor.EditorUtility.SetDirty(UnityEditor.Selection.activeObject);
        }
    }
}