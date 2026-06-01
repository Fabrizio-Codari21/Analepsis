using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject,IClue
{
    [Space(25), Header("CLUE DATA")]

    [Space(20)]
    [InfoBox("Si la scale es mas chico, el objeto se ve mas grande")]
    public float renderCameraScaleMax = 1;
    public float renderCameraScaleMin = 1;
    public ItemViewer gameObject;
    public SerializableGuid guid = SerializableGuid.NewGuid();

    public string Name;
    [PreviewField] public Sprite sprite;

    [Space(15), Header("WHAT CLUES CAN WE FIND?")]
    [TextArea(0, 20)] public string baseClue;

    public List<ItemPOIData> pois = new();

    [Space(15), Header("ON A FLASHBACK, YOU'LL SEE...")]
    public FlashbackInfo flashbackInfo;

    [Space(15), Header("WHAT DOES IT PROVE?")]
    [SerializeField] List<Whodunnit> doesItProveAnything;

    [Space(15), Header("CAN IT UNLOCK ANYTHING?")]
    public KeyItem keyInfo; // eventualmente esto podria guardar mas info relevante, por ahora es solo un bool.

    
}

[Serializable]
public class ItemPOIData
{
    public string poiId;
    [TextArea(0,10)]
    public string description;
    
    
}

public interface IClue
{
    
}

