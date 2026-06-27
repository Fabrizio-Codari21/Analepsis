using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class DialogueNode : INode,IClue
{
    [TextArea(0,20)] public string dialogueText;
    public Emotion characterEmotion;
    public Reaction characterReaction;
    public Whodunnit doesItProveAnything;
    public bool isKey;

    [SerializeReference] public List<DialogueResponse> responses;
    public bool isRootNode = true;
    [HideInInspector] public Vector2 editorPosition;
    [SerializeReference] public List<AltDialoguePath> altDialoguePaths = new List<AltDialoguePath>();

    [Header("ID")]
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public string tag = "";
    DialogueResponse _previousResponse = default; 
    public DialogueResponse PreviousResponse { get => _previousResponse; set => _previousResponse = value; }

    public DialogueNode SelectAltDialogue()
    {
        if (altDialoguePaths.All(x => x.condition == null) 
            //|| altDialoguePaths.All(x => x.dialogueReference == null)
            || altDialoguePaths.All(x => !DialogueManager.Instance.CheckDialogue(x.condition.guid))) 
            return this;

        // Por organizacion, elijo el ultimo de los dialogos que hayas creado en el graph
        // (habria que organizarse creandolos de menos a mas prioritario).
        var path = altDialoguePaths.Last(x => DialogueManager.Instance.CheckDialogue(x.condition.guid));

        var newNode = new DialogueNode();
        newNode.dialogueText = path.altDialogue;

        // Si el dialogo al que salteas tambien tiene un dialogo alternativo valido, saltea a ese directo.
        var skip = path.skipTo.SelectAltDialogue();
        newNode.responses = skip.responses;
        newNode.characterEmotion = skip.characterEmotion; 
        newNode.characterReaction = skip.characterReaction;

        return newNode;
    }

    public SerializableGuid CompareGuid()
    {
        return guid;
    }
}

public interface INode {}

[Serializable]
public class AltDialoguePath
{
    [TextArea(0, 20)] public string altDialogue;
    public DialogueNode condition;
    public DialogueNode skipTo;
}
