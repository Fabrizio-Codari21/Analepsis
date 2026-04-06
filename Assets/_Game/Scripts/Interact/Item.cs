using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject
{
    public GameObject gameObject;
    
    [MinValue(1)] public float size;

    [SerializeReference] public Clue clueInfo;
}

public class Clue
{
    public string clueId;
    public Sprite sprite;
    public GameObject clueDisplay;
    [TextArea(0,20)] public List<string> clues;
}