using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public sealed class DialogueGraphNode : Node
{
    public DialogueNode NodeData;
    public Port InputPort;
    public Port OutputPort;
    private DialogueGraphView _graphView;
    private AltDialogueSearchWindow _searchWindowProvider;
    private VisualElement altDialogueContainer;
    public Foldout nodeFoldOut;
    public Foldout altDialogueFoldOut;
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

        nodeFoldOut = new Foldout()
        {
            text = "Custom Details",
            value = false,
        };

        Foldout emotionFoldOut = new Foldout()
        {
            text = "How should they emote?",
            value = false,
        };

        PopupField<Emotion> emotionField =
        new PopupField<Emotion>("Pick an emotion")
        {
            value = nodeData.characterEmotion,
            choices = new List<Emotion>
            {
                Emotion.None,
                Emotion.Idle,
                Emotion.Worried,
                Emotion.Angry,
                Emotion.Happy,
                Emotion.Sad,
            }
        };

        emotionField.RegisterValueChangedCallback(evt =>
        {
            NodeData.characterEmotion = evt.newValue;
        });

        emotionFoldOut.Add(emotionField);
        nodeFoldOut.Add(emotionFoldOut);

        Foldout reactionFoldOut = new Foldout()
        {
            text = "Should this change their animation?",
            value = false,
        };

        PopupField<Reaction> reactionField =
        new PopupField<Reaction>("Pick a reaction")
        {
            value = nodeData.characterReaction,
            choices = new List<Reaction>
            {
                Reaction.None,
                Reaction.Idle,
                Reaction.Gesticulate,
                Reaction.AvoidGaze,
                Reaction.Laugh,
                Reaction.GetNervous,
                Reaction.Think,
                Reaction.Generic,
                Reaction.Angry,
            }
        };

        reactionField.RegisterValueChangedCallback(evt =>
        {
            NodeData.characterReaction = evt.newValue;
        });

        reactionFoldOut.Add(reactionField);
        nodeFoldOut.Add(reactionFoldOut);
        
        Foldout proofFoldOut = new Foldout()
        {
            text = "What can it prove?",
            value = false,
        };
        
        EnumFlagsField proofField = new EnumFlagsField("What can it prove?", nodeData.doesItProveAnything);
        
      

        proofField.RegisterValueChangedCallback(evt =>
        {
            NodeData.doesItProveAnything = (Whodunnit)evt.newValue;
        });

        proofFoldOut.Add(proofField);
        nodeFoldOut.Add(proofFoldOut);

        Foldout isKeyFoldOut = new Foldout()
        {
            text = "Is this a Key?",
            value = false,
        };

        Toggle isKeyField = new Toggle("Is Key")
        {
            value = false,
        };
        isKeyField.RegisterValueChangedCallback(evt =>
        {
            NodeData.isKey = evt.newValue;
        });

        isKeyFoldOut.Add(isKeyField);
        nodeFoldOut.Add(isKeyFoldOut);

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
        else
        {
            // Para evitar complicaciones, podemos hacer que los dialogos alternativos solo se puedan
            // asignar al nodo inicial.
            //altDialogueFoldOut = new Foldout()
            //{
            //    text = "Could this dialogue change?",
            //    value = false,
            //};
            //GenerateAltDialogueUI();

        }

        altDialogueFoldOut = new Foldout()
        {
            text = "Could this dialogue change?",
            value = false,
        };
        GenerateAltDialogueUI();

        extensionContainer.Add(nodeFoldOut);

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

    public void GenerateAltDialogueUI()
    {
        altDialogueContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                backgroundColor = new Color(.2f, 0f, 0.8f, 0.3f),
                paddingLeft = 12,
                paddingTop = 12,
                paddingRight = 12,
                paddingBottom = 10,
                marginRight = 20,
            }
        };

        //SerializedObject pathClass = new SerializedObject(nodeData.altDialoguePaths);
        //SerializedProperty paths = pathClass.FindProperty("Paths");

        //PropertyField altDialogueField = new PropertyField(paths)
        //{
        //    name = "Add alternative dialogues",
        //};
        //altDialogueField.Bind(pathClass);

        //if(NodeData.altDialoguePath is AltDialoguePath path) // para volver rapido a que sea solo con un altDialogue
        foreach (AltDialoguePath path in NodeData.altDialoguePaths)
        {
            Func<DialogueNode, string> formatLabel = n => {
                if (n == null) return "Select a Node";
                string preview = string.IsNullOrEmpty(n.dialogueText) ? "Empty" : n.dialogueText;
                if (preview.Length > 50) preview = preview[..50] + "...";
                return $"[{n.tag}] {preview}";
            };
            var initialChoices = _graphView.CurrentDialogue?.allNodes ?? new List<DialogueNode>();

            PopupField<DialogueNode> condNodeField =
            new PopupField<DialogueNode>("If we've reached this point...",
                initialChoices,
                0,
                formatLabel,
                formatLabel
            )
            { value = path.condition };
            condNodeField.RegisterValueChangedCallback(evt =>
            {
                path.condition = evt.newValue;
            });

            PopupField<DialogueNode> skipToField =
            new PopupField<DialogueNode>("... we skip to this point...",
                initialChoices,
                0,
                formatLabel,
                formatLabel
            )
            { value = path.skipTo };
            skipToField.RegisterValueChangedCallback(evt =>
            {
                path.skipTo = evt.newValue;
            });

            // esto despues tiene que cambiarse
            //ObjectField dialogueRefField = new ObjectField("Select dialogue (placeholder)")
            //{
            //    objectType = typeof(Dialogue),
            //    value = _graphView.CurrentDialogue,
            //};
            //dialogueRefField.RegisterValueChangedCallback(evt =>
            //{
            //    path.dialogueReference = (Dialogue)evt.newValue;
            //    UpdatePopupOptions();
            //    EditorUtility.SetDirty(Selection.activeObject);
            //});

            TextField altDialogueField = new TextField("... and have them say:")
            {
                multiline = true,
                value = path.altDialogue,
                style = { paddingBottom = 10, }
            };
            altDialogueField.RegisterValueChangedCallback(evt =>
            {
                path.altDialogue = evt.newValue;
            });


            void UpdatePopupOptions()
            {
                if (_graphView.CurrentDialogue != null
                    && _graphView.CurrentDialogue.allNodes != null)
                {
                    var nodes = _graphView.CurrentDialogue.allNodes;

                    condNodeField.choices = nodes;
                    skipToField.choices = nodes;

                    if (path.condition != null && nodes.Contains(path.condition))
                        condNodeField.value = path.condition;
                    else if (nodes.Count > 0) condNodeField.index = 0;

                    if (path.skipTo != null && nodes.Contains(path.skipTo))
                        skipToField.value = path.skipTo;
                    else if (nodes.Count > 0) skipToField.index = 0;
                }
                else
                {
                    condNodeField.choices = new List<DialogueNode>();
                    condNodeField.value = null;

                    skipToField.choices = new List<DialogueNode>();
                    skipToField.value = null;
                }
            }

            VisualElement column = new VisualElement { style = { flexDirection = FlexDirection.Column, marginBottom = 5 } };

            VisualElement header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    paddingBottom = 25,
                }
            };

            Label mainLabel = new Label("<b>(Note: if there are multiple dialogues, higher index = higher priority.)</b>")
            {
                style = { paddingBottom = 25, fontSize = 13 }
            };
            Label elementLabel = new Label($"<b>Dialogue {NodeData.altDialoguePaths.IndexOf(path)}</b>")
            {
                style = { paddingBottom = 10, fontSize = 10 }
            };

            VisualElement buttonRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                } 
            };

            var index = NodeData.altDialoguePaths.IndexOf(path);
            Button moveUpBtn = new Button(() => {
                NodeData.altDialoguePaths.TrySwap(index, index - 1, out var exc);
                GenerateAltDialogueUI();
                EditorUtility.SetDirty(Selection.activeObject);
            })
            { text = "<<< Lower Priority"};
            Button moveDownBtn = new Button(() => {
                NodeData.altDialoguePaths.TrySwap(index, index + 1, out var exc);
                GenerateAltDialogueUI();
                EditorUtility.SetDirty(Selection.activeObject);
            })
            { text = "Increase Priority >>>"};
            //Button topBtn = new Button(() => {
            //    var index = NodeData.altDialoguePaths.IndexOf(path);
            //    NodeData.altDialoguePaths.TrySwap(index, 0, out var exc);
            //    GenerateAltDialogueUI();
            //    EditorUtility.SetDirty(Selection.activeObject);
            //})
            //{ text = "MIN Priority" };
            //Button bottomBtn = new Button(() => {
            //    var index = NodeData.altDialoguePaths.IndexOf(path);
            //    NodeData.altDialoguePaths.TrySwap(index, NodeData.altDialoguePaths.Count - 1, out var exc);
            //    GenerateAltDialogueUI();
            //    EditorUtility.SetDirty(Selection.activeObject);
            //})
            //{ text = "MAX Priority" };


            Button removeBtn = new Button(() => {
                NodeData.altDialoguePaths.Remove(path);
                GenerateAltDialogueUI();
                EditorUtility.SetDirty(Selection.activeObject);
            })
            { text = "X" };

            if(NodeData.altDialoguePaths.First() != path) column.Add(header);
            else if (NodeData.altDialoguePaths.Count > 1) column.Add(mainLabel);

            //column.Add(dialogueRefField);
            column.Add(elementLabel);
            column.Add(condNodeField);
            column.Add(skipToField);
            column.Add(altDialogueField);

            if (NodeData.altDialoguePaths.Count > 1)
            {
                if(NodeData.altDialoguePaths.First() != path)
                {
                    //if(NodeData.altDialoguePaths.Count > 2) buttonRow.Add(topBtn);
                    buttonRow.Add(moveUpBtn);
                }
                if (NodeData.altDialoguePaths.Last() != path)
                {
                    buttonRow.Add(moveDownBtn);
                    //if (NodeData.altDialoguePaths.Count > 2) buttonRow.Add(bottomBtn);
                }
            }
            column.Add(buttonRow);

            column.Add(removeBtn);

            altDialogueContainer.Add(column);
        }


        Button addDialogueButton = new Button(AddAltDialogue)
        { text = "+ Add Alt Dialogue", style = { marginRight = 25, } };
        altDialogueFoldOut.Clear();
        altDialogueFoldOut.Add(altDialogueContainer);
        altDialogueFoldOut.Add(addDialogueButton);
        nodeFoldOut.Add(altDialogueFoldOut);
    }

    private void AddAltDialogue()
    {
        Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        _graphView.OpenAltDialogueSearchWindow(this, mousePos);
        //extensionContainer.GetFirstOfType<Foldout>().text = $"Conditions ({ResponseData.m_conditions.Count})";
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