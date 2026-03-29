using UnityEngine;

// Lo que decimos en respuesta a un NPC y que dialogo le sigue a esa respuesta.
[System.Serializable]
public class DialogueResponse
{
    [TextArea] public string responseText;
    [SerializeReference] public DialogueNode nextNode;
    //public string nextNodeId; // referencia al identificador del proximo nodo;
}
