using System.Collections.Generic;
using Unity.Collections;
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
    public DialogueNode startingNode;
    [ReadOnly] string _dialogueLog;
    //public List<DialogueNode> dialogueNodes;
    public void AddToLog(string speakerName, string newLine) => _dialogueLog += (_dialogueLog != "" ? "\n" : "")  + $"{speakerName}: - {newLine}";
    public void DeleteLog() => _dialogueLog = "";
    public string GetLog() => _dialogueLog;


}
