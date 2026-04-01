using System.Collections.Generic;
using UnityEngine;
// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class DialogueNode
{
    //public string id; // identificador para cada nodo
    [TextArea(0,20)] public string dialogueText;
    [SerializeReference] public List<DialogueResponse> responses;

    public bool isRootNode = false;
    public Vector2 editorPosition;
    internal bool IsLastNode()
    {
        return responses.Count <= 0;
    }
}
