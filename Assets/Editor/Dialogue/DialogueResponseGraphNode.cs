using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class DialogueResponseGraphNode : Node
{
    public DialogueResponse ResponseData;
    private DialogueGraphView _graphView;
    public Port InputPort;
    public Port OutputPort;

    public DialogueResponseGraphNode(DialogueResponse responseData, DialogueGraphView graphView)
    {
        _graphView = graphView;
        ResponseData = responseData;

        capabilities |= Capabilities.Deletable;
        capabilities |= Capabilities.Selectable;
        capabilities |= Capabilities.Movable;
        title = "Response";

        TextField responseField = new TextField("Response")
        {
            multiline = true,
            value = responseData.responseText
        };

        responseField.RegisterValueChangedCallback(evt =>
        {
            ResponseData.responseText = evt.newValue;
        });

        extensionContainer.Add(responseField);

        InputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Single,
            typeof(bool)
        );

        InputPort.portName = "From Dialogue";
        inputContainer.Add(InputPort);
        InputPort.portColor = Color.cyan;
        OutputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Single,
            typeof(bool)
        );

        OutputPort.portName = "To Dialogue";
        OutputPort.portColor = Color.cyan;
        outputContainer.Add(OutputPort);
        EdgeConnector<Edge> edgeConnector =
            new EdgeConnector<Edge>(new DialogueEdgeConnectorListener(_graphView));

        OutputPort.AddManipulator(edgeConnector);
        RefreshExpandedState();
        RefreshPorts();
    }
    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);

        ResponseData.editorPosition = newPos.position;

        if (UnityEditor.Selection.activeObject != null)
        {
            UnityEditor.EditorUtility.SetDirty(UnityEditor.Selection.activeObject);
        }
    }
   
}