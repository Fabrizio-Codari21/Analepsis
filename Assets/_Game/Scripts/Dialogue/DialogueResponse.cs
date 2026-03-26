using UnityEngine;

// Lo que decimos en respuesta a un NPC y que dialogo le sigue a esa respuesta.
[System.Serializable]
public class DialogueResponse : MonoBehaviour
{
    public string responseText;
    public DialogueNode nextNode;
}
