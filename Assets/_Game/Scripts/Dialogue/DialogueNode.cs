using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class DialogueNode : INode
{
    [TextArea(0,20)] public string dialogueText;
    public Emotion characterEmotion;
    public Reaction characterReaction;
    public Whodunnit doesItProveAnything;
    public bool isKey;

    [SerializeReference] public List<DialogueResponse> responses;
    public bool isRootNode = true;
    [HideInInspector] public Vector2 editorPosition;
    [ShowIf("isRootNode")] public AltDialoguePath altDialoguePath;

    [Header("ID")]
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public string tag = "";
    DialogueResponse _previousResponse = default; 
    public DialogueResponse PreviousResponse { get => _previousResponse; set => _previousResponse = value; }

    public DialogueNode SelectAltDialogue()
    {
        if (altDialoguePath.condition == null || altDialoguePath.dialogueReference == null
            || !DialogueManager.Instance.CheckDialogue(altDialoguePath.condition.guid)) 
            return this;

        var newNode = new DialogueNode();
        newNode.dialogueText = altDialoguePath.altDialogue;
        newNode.responses = altDialoguePath.skipTo.responses;
        newNode.characterEmotion = altDialoguePath.skipTo.characterEmotion; 
        newNode.characterReaction = altDialoguePath.skipTo.characterReaction;

        return newNode;
    }

}

public interface INode {}

[Serializable]
public struct AltDialoguePath
{
    [TextArea(0, 20)] public string altDialogue;
    public Dialogue dialogueReference; //placeholder, esto despues se tendria que cambiar.
    public DialogueNode condition;
    public DialogueNode skipTo;
}
