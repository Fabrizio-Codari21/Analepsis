using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject
{
    public GameObject gameObject;
    [SerializeReference] public Clue itemClue;
}

public class Clue 
{
    [TextArea(0, 20)] public List<string> clues;
    public GameObject itemDisplay;
}