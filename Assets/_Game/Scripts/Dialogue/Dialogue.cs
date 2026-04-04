using System.Collections.Generic;
using UnityEngine;

// Objeto base que contiene toda la logica de un dialogo.
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue Assets/New Dialogue")]
public class Dialogue : ScriptableObject
{
    [Space(20)]
    [Header("DIALOGUE")]
    public DialogueNode startingNode;
    public List<DialogueNode> allNodes =  new List<DialogueNode>();
}