using System;
using System.Collections.Generic;
//using System.ComponentModel;
using Sirenix.OdinInspector;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

// Objeto base que contiene toda la logica de un dialogo.
[Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue Assets/New Dialogue")]
public class Dialogue : ScriptableObject, IClue
{
    [Space(20)]
    [Header("DIALOGUE")]
    public Color dialogueColor;
    public DialogueNode startingNode;
    public List<DialogueNode> allNodes =  new List<DialogueNode>();

    [ReadOnly] public List<Whodunnit> _hiddenProof = new();
    public void DiscoverProof(Whodunnit proof)
    {
        if (_hiddenProof.Contains(proof)) return;
        _hiddenProof.Add(proof);
    }

    public List<Whodunnit> DoesItProveAnything()
    {
        return new List<Whodunnit>(_hiddenProof);

    }
}