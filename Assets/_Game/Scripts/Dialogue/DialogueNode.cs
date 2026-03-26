using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lo que pueden decir los NPC y como podemos responder a eso.
[System.Serializable]
public class DialogueNode : MonoBehaviour
{
    public string dialogueText;
    public List<DialogueResponse> responses;

    internal bool IsLastNode()
    {
        return responses.Count <= 0;
    }
}
