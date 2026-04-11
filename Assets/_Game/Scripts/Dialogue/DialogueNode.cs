using System.Collections.Generic;
using UnityEngine;
// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class DialogueNode : IClue
{
    [TextArea(0,20)] public string dialogueText;
    public TheoryboardManager.Whodunnit doesItProveAnything;

    [SerializeReference] public List<DialogueResponse> responses;
    public bool isRootNode = true;
    [HideInInspector] public Vector2 editorPosition;

    [Header("ID")]
    public SerializableGuid guid = SerializableGuid.NewGuid();
    public string tag = "";

    public TheoryboardManager.Whodunnit DoesItProveAnything()
    {
        return doesItProveAnything;    
    }
}
