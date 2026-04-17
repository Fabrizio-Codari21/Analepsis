using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject, IClue
{
    public GameObject gameObject;
    
    public SerializableGuid guid = SerializableGuid.NewGuid();
    [MinValue(1)] public float size = 1;
    public string Name;
    public Sprite sprite;

    [Space(15), Header("WHAT CLUES CAN WE FIND?")]
    // La lista de textos sirve para poder tener la data de los puntos de interes por separado.
    [TextArea(0, 20)] public List<string> itemClues = new();
    [HideInInspector] public string flashbackClue;

    [Space(15), Header("ON A FLASHBACK, YOU'LL SEE...")]
    public FlashbackInfo flashbackInfo;

    [Space(15), Header("WHAT DOES IT PROVE?")]
    [SerializeField] List<TheoryboardManager.Whodunnit> doesItProveAnything;

    public List<TheoryboardManager.Whodunnit> DoesItProveAnything()
    {
        return new List<TheoryboardManager.Whodunnit>(doesItProveAnything);
    }
}


public interface IClue
{
    public List<TheoryboardManager.Whodunnit> DoesItProveAnything();
}

