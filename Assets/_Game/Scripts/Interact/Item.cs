using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject
{
    public GameObject gameObject;
    [SerializeReference] public Clue clueInfo;

    [MinValue(1)] public float size;
}

public class Clue
{
    [TextArea(0,20)] public List<string> clues;
    public GameObject clueDisplay;
}