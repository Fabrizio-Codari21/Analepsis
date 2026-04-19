using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject, IClue
{
    public ItemViewer gameObject;
    public SerializableGuid guid = SerializableGuid.NewGuid();
    [InfoBox("Si la scale es mas chico, el objeto se ve mas grande")]
    public float renderCameraScaleMax = 1;
    public float renderCameraScaleMin = 1;
    
    public string Name;
    public Sprite sprite;

    [Space(15), Header("WHAT CLUES CAN WE FIND?")]
    // La lista de textos sirve para poder tener la data de los puntos de interes por separado.
    [TextArea(0, 20)] public string baseClue;


    [Space(15), Header("ON A FLASHBACK, YOU'LL SEE...")]
    public FlashbackInfo flashbackInfo ;

    [Space(15), Header("WHAT DOES IT PROVE?")]
    [SerializeField] List<Whodunnit> doesItProveAnything;

    public List<Whodunnit> DoesItProveAnything()
    {
        return new List<Whodunnit>(doesItProveAnything);
    }
    
    public List<ItemPOIData> pois = new();
}

[Serializable]
public class ItemPOIData
{
    public string poiId;
    [TextArea]
    public string description;
    
    
}


public interface IClue
{
    public List<Whodunnit> DoesItProveAnything();
}

