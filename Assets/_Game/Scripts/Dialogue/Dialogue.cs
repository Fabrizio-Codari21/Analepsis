using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

using UnityEngine;

// Objeto base que contiene toda la logica de un dialogo.
[Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue Assets/New Dialogue")]
public class Dialogue : Clue
{
    [Space(25), Header("CLUE DATA")]
    [Space(20)]
    [Header("DIALOGUE")]
    public Color dialogueColor;
    public DialogueNode startingNode;
    public List<DialogueNode> allNodes =  new List<DialogueNode>();

    [ReadOnly] public List<Whodunnit> hiddenProof = new();
    public void DiscoverProof(Whodunnit proof)
    {
        if (hiddenProof.Contains(proof)) return;
        hiddenProof.Add(proof);
    }

  
    

}