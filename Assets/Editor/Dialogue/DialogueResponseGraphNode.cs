using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class DialogueResponseGraphNode : Node
{
    public DialogueResponse ResponseData;
    private DialogueGraphView _graphView;
    public Port InputPort;
    public Port OutputPort;

    private VisualElement conditionContainer;
    private ConditionSearchWindow _searchWindowProvider;
    public DialogueResponseGraphNode(DialogueResponse responseData, DialogueGraphView graphView)
    {
        _graphView = graphView;
        ResponseData = responseData;

        capabilities |= Capabilities.Deletable;
        capabilities |= Capabilities.Selectable;
        capabilities |= Capabilities.Movable;
        title = "Response";
        titleContainer.style.backgroundColor = new Color(0.12f, 0.45f, 0.25f, 0.8f);
        #region ResponseText
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
        
        #endregion
        
        #region Condition Area
        Foldout conditionFoldout = new Foldout()
        {
            text = $"Conditions ({responseData.m_conditions.Count})",
            value = false ,
            
        };
        conditionContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                backgroundColor = new Color(.8f, 0f, 0.2f, 0.3f),
                paddingLeft = 10,
                paddingTop = 10,
                paddingRight = 10,
                paddingBottom = 10,
            }
        };
        conditionFoldout.Add(conditionContainer);
        Button addConditionButton = new Button (AddCondition) 
            { text = "+ Add Condition" };
        conditionFoldout.Add(addConditionButton);
        extensionContainer.Add(conditionFoldout);
        #endregion

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
        EdgeConnector<Edge> edgeConnector = new EdgeConnector<Edge>(new DialogueEdgeConnectorListener(_graphView));
        OutputPort.AddManipulator(edgeConnector);
        GenerateConditionUI();
        RefreshExpandedState();
        RefreshPorts();
    }


    public void GenerateConditionUI()
    {
        conditionContainer.Clear();
        if (ResponseData.m_conditions == null) return;

        foreach (var condition in ResponseData.m_conditions)
        {
            VisualElement row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 5 } };
            
            VisualElement header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    
                }
                
            };

            // si es DialogueNodeCondition , me da paja de hacer "bien solid", asi que algo mas facil posible, vamos hacer uno por uno los conditiones posible
            if (condition is DialogueNodeCondition nodeCond)
            {
                ObjectField dialogueAssetField = new ObjectField("Target Dialogue")
                {
                    objectType = typeof(Dialogue),
                    value = nodeCond.targetDialogue,
                };

                Toggle unlockIfTrueField = new Toggle("Unlock if True")
                {
                    value = nodeCond.unlockIfTrue,
                };

                Func<DialogueNode, string> formatLabel = n => {
                    if (n == null) return "Select a Node";
                    string preview = string.IsNullOrEmpty(n.dialogueText) ? "Empty" : n.dialogueText;
                    if (preview.Length > 15) preview = preview[..15] + "...";
                    return $"[{n.tag}] {preview}";
                };
                var initialChoices = nodeCond.targetDialogue?.allNodes ?? new List<DialogueNode>();
                PopupField<DialogueNode> nodeSelector = new PopupField<DialogueNode>(
                    "Target Node",
                    initialChoices,
                    0,
                    formatLabel, 
                    formatLabel  
                );

                void UpdatePopupOptions()
                {
                    if (nodeCond.targetDialogue != null && nodeCond.targetDialogue.allNodes != null)
                    {
                        var nodes = nodeCond.targetDialogue.allNodes;

                        nodeSelector.choices = nodes;

                        if (nodeCond.isTalkDialogueNode != null && nodes.Contains(nodeCond.isTalkDialogueNode))
                            nodeSelector.value = nodeCond.isTalkDialogueNode;
                        else if (nodes.Count > 0) nodeSelector.index = 0;
                    }
                    else
                    {
                        nodeSelector.choices = new List<DialogueNode>();
                        nodeSelector.value = null;
                    }
                }

                dialogueAssetField.RegisterValueChangedCallback(evt => {
                    nodeCond.targetDialogue = (Dialogue)evt.newValue;
                    UpdatePopupOptions();
                    EditorUtility.SetDirty(Selection.activeObject);
                });

                nodeSelector.RegisterValueChangedCallback(evt => {
                    nodeCond.isTalkDialogueNode = evt.newValue;
                    EditorUtility.SetDirty(Selection.activeObject);
                });

                unlockIfTrueField.RegisterValueChangedCallback(evt =>
                {
                    nodeCond.unlockIfTrue = evt.newValue;
                    EditorUtility.SetDirty(Selection.activeObject);
                });

                UpdatePopupOptions();
                row.Add(dialogueAssetField);
                row.Add(nodeSelector);
                row.Add(unlockIfTrueField);
            }
            
            if (condition is ItemNodeCondition itemCond)
            {
                ObjectField itemAssetField = new ObjectField("Required Item")
                {
                    objectType = typeof(Item), 
                    value = itemCond.item,
                    style = { flexGrow = 1 }
                };

                Toggle unlockIfTrueField = new Toggle("Unlock if True")
                {
                    value = itemCond.unlockIfTrue,
                };

                itemAssetField.RegisterValueChangedCallback(evt => {
                    itemCond.item = (Item)evt.newValue;
                    EditorUtility.SetDirty(Selection.activeObject);
                });

                unlockIfTrueField.RegisterValueChangedCallback(evt =>
                {
                    itemCond.unlockIfTrue = evt.newValue;
                    EditorUtility.SetDirty(Selection.activeObject);
                });

                row.Add(itemAssetField);
                row.Add(unlockIfTrueField);
            }
        
            Button removeBtn = new Button(() => {
                ResponseData.m_conditions.Remove(condition);
                GenerateConditionUI();
                EditorUtility.SetDirty(Selection.activeObject);
            }) { text = "X" };
            
            row.Add(header);
            row.Add(removeBtn);
            conditionContainer.Add(row); 
        }
        RefreshExpandedState();
    }
    private void AddCondition()
    {
        Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        _graphView.OpenConditionSearchWindow(this, mousePos);
        //extensionContainer.GetFirstOfType<Foldout>().text = $"Conditions ({ResponseData.m_conditions.Count})";
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