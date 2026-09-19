using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
public class DialogueEdgeConnectorListener : IEdgeConnectorListener
{
    private DialogueGraphView graphView;

    public DialogueEdgeConnectorListener(DialogueGraphView graphView)
    {
        this.graphView = graphView;
    }

    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        graphView.OpenSearchWindow(edge.output, position);
        Debug.Log("Drop Outside Port");
    }

    public void OnDrop(GraphView graphView, Edge edge)
    {
        graphView.AddElement(edge);
        Debug.Log("Add");
    }
}