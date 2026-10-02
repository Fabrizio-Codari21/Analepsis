using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PlayerResponse : Node
{
    public DialogueResponse ResponseData;
    private DialogueGraphView _graphView;
    public Port InputPort;
    public Port OutputPort;

    private VisualElement conditionRoot;
    private VisualElement conditionContainer;
    private ConditionSearchWindow _searchWindowProvider;
    private VisualElement superiorContainer;
    private VisualElement bottomContainer;
   public PlayerResponse(DialogueResponse responseData, DialogueGraphView graphView)
    {
        _graphView = graphView;
        ResponseData = responseData;

        capabilities |= Capabilities.Deletable;
        capabilities |= Capabilities.Selectable;
        capabilities |= Capabilities.Movable;
        capabilities |= Capabilities.Resizable;

        title = "Player Response";
        

        #region Port Setup
        
        superiorContainer = new VisualElement{
            style =
            {
                flexDirection = FlexDirection.Row,
                backgroundColor = Color.gray,
                paddingLeft = 10,
                paddingTop = 10,
                paddingRight = 10,
                paddingBottom = 10,
            }
        };
        
        var listener = new DialogueEdgeConnectorListener(_graphView);
        InputPort = Port.Create<Edge>(Orientation.Vertical, Direction.Input, Port.Capacity.Multi,typeof(NpcResponse));
        InputPort.portName = "";
        InputPort.portColor = Color.cyan;
        
        InputPort.AddManipulator(new EdgeConnector<Edge>(listener));
        Label inputPortLabel = new Label
        {
            text = "Response Npc",
            style =
            {
                color = Color.black,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };
        inputContainer.Add(InputPort);
        inputContainer.Add(inputPortLabel);
        
        
  
        OutputPort = Port.Create<Edge>(
            Orientation.Vertical,
            Direction.Output,
            Port.Capacity.Multi,
            typeof(NpcResponse)
        );
        OutputPort.portName = "";
        OutputPort.portColor = Color.cyan;
        OutputPort.AddManipulator(new EdgeConnector<Edge>(listener));
        Label outPortLabel = new Label
        {
            text = "Npc Response After This",
            style =
            {
                color = Color.black,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };
        
        outputContainer.Add(outPortLabel);
        outputContainer.Add(OutputPort);
      
        outputContainer.style.flexDirection = FlexDirection.Column;
        
      
        
        #endregion

        #region ResponseText
        TextField responseField = new TextField("Response")
        {
            multiline = true,
            value = responseData.responseText,
            style =
            {
                flexDirection = FlexDirection.Column,
                width = Length.Percent(100),
                height = 200,
                whiteSpace = WhiteSpace.Normal,
                marginBottom = 20,
                marginLeft = 20,
                marginRight = 20,
            }
        };

        responseField.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
        responseField.labelElement.style.height = 20;
        responseField.labelElement.style.marginBottom = 5;
        responseField.labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;

        responseField.RegisterValueChangedCallback(evt => { responseData.responseText = evt.newValue; });

        extensionContainer.Add(responseField);
        #endregion

        #region Condition Area
        Foldout conditionFoldout = new Foldout()
        {
            text = $"Conditions ({responseData.m_conditions.Count})",
            value = false,
            
        };
        
        var foldoutToggle = conditionFoldout.Q<Toggle>();
        var foldoutLabel = conditionFoldout.Q<Label>();
        
        foldoutToggle.style.minHeight = 36; 
        foldoutToggle.style.paddingTop = 6;
        foldoutToggle.style.paddingBottom = 6;
        foldoutToggle.style.paddingLeft = 6;
        foldoutToggle.style.justifyContent = Justify.Center;

        if (foldoutLabel != null)
        {
            foldoutLabel.style.fontSize = 14; 
            foldoutLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        var checkmark = conditionFoldout.Q(className: "unity-foldout__toggle").Q(className: "unity-toggle__checkmark");
        if (checkmark != null)
        {
            checkmark.style.scale = new Scale(new Vector3(1.3f, 1.3f, 1f));
        }

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
        Button addConditionButton = new Button(AddCondition) { text = "+ Add Condition" };
        conditionFoldout.Add(addConditionButton);

        conditionRoot = new VisualElement()
        {
            style =
            {
                backgroundColor = new Color(245f, 245f, 220f, 0.3f),
            }
        };
        
        conditionRoot.Add(conditionFoldout);
        extensionContainer.Add(conditionRoot);
        #endregion

        GenerateConditionUI();
        RefreshExpandedState();
        RefreshPorts();

        
        #region Reorder to #node-border (Input -> Title -> Extension -> Output)

        VisualElement border = Children().First();
       
        inputContainer.RemoveFromHierarchy();
        outputContainer.RemoveFromHierarchy();
        
      
       
        border.Insert(0, superiorContainer);

        Button deleteButon = new Button(()=>graphView.DeleteElements(new List<GraphElement> { this }))
        {
            text = "X",
            style =
            {
                backgroundColor = Color.red,
                color = Color.white
            }
        };
        
        superiorContainer.style.justifyContent = Justify.SpaceBetween;
        superiorContainer.style.alignItems = Align.Center;

        
        superiorContainer.Add(inputContainer);
        superiorContainer.Add(deleteButon);


        inputContainer.style.flexGrow = 1;

        deleteButon.style.alignSelf = Align.Center;
        deleteButon.style.flexShrink = 0;
        
        
        bottomContainer= new VisualElement{
            style =
            {
                backgroundColor = Color.gray,
                paddingLeft = 10,
                paddingTop = 10,
                paddingRight = 10,
                paddingBottom = 10,
            }
        };
        
        bottomContainer.Add(outputContainer);
        border.Add(bottomContainer);

           
        #endregion
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
                )
                { value = nodeCond.isTalkDialogueNode };

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