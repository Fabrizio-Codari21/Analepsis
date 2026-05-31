using System.Collections.Generic;
using UnityEngine;
// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class DialogueNode : INode,IClue
{
    [TextArea(0,20)] public string dialogueText;
    public Emotion characterEmotion;
    public Reaction characterReaction;
    public Whodunnit doesItProveAnything;

    [SerializeReference] public List<DialogueResponse> responses;
    public bool isRootNode = true;
    [HideInInspector] public Vector2 editorPosition;

    [Header("ID")]
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public string tag = "";
    private DialogueResponse _previousResponse = null;
    
    public DialogueResponse PreviousResponse
    {
        get => _previousResponse;
        set => _previousResponse = value;
    }
    
}

public interface INode {}
