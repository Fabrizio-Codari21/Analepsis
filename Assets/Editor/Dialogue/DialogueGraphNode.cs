using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

        Foldout nodeFoldOut = new Foldout()
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

        PopupField<Whodunnit> proofField = 
        new PopupField<Whodunnit>("Select proof")
        {
            value = nodeData.doesItProveAnything,
            choices = new List<Whodunnit> 
            { 
                Whodunnit.NoProof,
                Whodunnit.Victim,
                Whodunnit.Killer,
                Whodunnit.Motive,
                Whodunnit.Weapon,
                Whodunnit.Place,
            }
        };

        proofField.RegisterValueChangedCallback(evt =>
        {
            NodeData.doesItProveAnything = evt.newValue;
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

        extensionContainer.Add(nodeFoldOut);

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
            // Para evitar complicaciones, hice que los dialogos alternativos solo se puedan asignar
            // al nodo inicial por ahora.
            Foldout altDialogueFoldOut = new Foldout()
            {
                text = "Could this dialogue change?",
                value = false,
            };

            //SerializedObject pathClass = new SerializedObject(nodeData.altDialoguePaths);
            //SerializedProperty paths = pathClass.FindProperty("Paths");

            //PropertyField altDialogueField = new PropertyField(paths)
            //{
            //    name = "Add alternative dialogues",
            //};
            //altDialogueField.Bind(pathClass);

            Func<DialogueNode, string> formatLabel = n => {
                if (n == null) return "Select a Node";
                string preview = string.IsNullOrEmpty(n.dialogueText) ? "Empty" : n.dialogueText;
                if (preview.Length > 50) preview = preview[..50] + "...";
                return $"[{n.tag}] {preview}";
            };
            var initialChoices = nodeData.altDialoguePath.dialogueReference?.allNodes ?? new List<DialogueNode>();

            PopupField<DialogueNode> condNodeField =
            new PopupField<DialogueNode>("If we've reached this point...", 
                initialChoices,
                0,
                formatLabel,
                formatLabel
            ){ value = nodeData.altDialoguePath.condition };
            condNodeField.RegisterValueChangedCallback(evt =>
            {
                NodeData.altDialoguePath.condition = evt.newValue;
            });

            PopupField<DialogueNode> skipToField =
            new PopupField<DialogueNode>("... we skip to this point...",
                initialChoices,
                0,
                formatLabel,
                formatLabel
            )
            { value = nodeData.altDialoguePath.skipTo };
            skipToField.RegisterValueChangedCallback(evt =>
            {
                NodeData.altDialoguePath.skipTo = evt.newValue;
            });

            // esto despues tiene que cambiarse
            ObjectField dialogueRefField = new ObjectField("Select dialogue (placeholder)")
            {
                objectType = typeof(Dialogue),
                value = nodeData.altDialoguePath.dialogueReference,
            };
            dialogueRefField.RegisterValueChangedCallback(evt =>
            {
                NodeData.altDialoguePath.dialogueReference = (Dialogue)evt.newValue;
                UpdatePopupOptions();
                EditorUtility.SetDirty(Selection.activeObject);
            });

            TextField altDialogueField = new TextField("... and have them say:")
            {
                multiline = true,
                value = nodeData.altDialoguePath.altDialogue,
            };
            altDialogueField.RegisterValueChangedCallback(evt =>
            {
                NodeData.altDialoguePath.altDialogue = evt.newValue;
            });


            void UpdatePopupOptions()
            {
                if (nodeData.altDialoguePath.dialogueReference != null
                    && nodeData.altDialoguePath.dialogueReference.allNodes != null)
                {
                    var nodes = nodeData.altDialoguePath.dialogueReference.allNodes;

                    condNodeField.choices = nodes;
                    skipToField.choices = nodes;

                    if (nodeData.altDialoguePath.condition != null && nodes.Contains(nodeData.altDialoguePath.condition))
                        condNodeField.value = nodeData.altDialoguePath.condition;
                    else if (nodes.Count > 0) condNodeField.index = 0;

                    if (nodeData.altDialoguePath.skipTo != null && nodes.Contains(nodeData.altDialoguePath.skipTo))
                        skipToField.value = nodeData.altDialoguePath.skipTo;
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

            altDialogueFoldOut.Add(dialogueRefField);
            altDialogueFoldOut.Add(condNodeField);
            altDialogueFoldOut.Add(skipToField);
            altDialogueFoldOut.Add(altDialogueField);
            nodeFoldOut.Add(altDialogueFoldOut);
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