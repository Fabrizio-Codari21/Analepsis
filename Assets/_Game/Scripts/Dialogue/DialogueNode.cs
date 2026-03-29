using System.Collections.Generic;
using UnityEngine;

// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class DialogueNode
{
    //public string id; // identificador para cada nodo
    [TextArea] public string dialogueText;
    [SerializeReference] public List<DialogueResponse> responses;

    internal bool IsLastNode()
    {
        return responses.Count <= 0;
    }
}
