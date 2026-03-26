using System.Collections.Generic;
using UnityEngine;

// Objeto base que contiene toda la logica de un dialogo.
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue Assets/New Dialogue")]
public class Dialogue : ScriptableObject
{
    //public DialogueNode startingNode;
    [Space(10)]
    [Header("CHARACTER SPEECH")]
    public Color characterTextColor;
    public float characterTalkingSpeed = 2;
    
    [Space(20)]
    [Header("DIALOGUE")]
    public List<DialogueNode> dialogueNodes;
}
