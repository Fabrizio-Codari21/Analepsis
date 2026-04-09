using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject
{
    public GameObject gameObject;
    
    public SerializableGuid guid = SerializableGuid.NewGuid();
    [MinValue(1)] public float size;
    public string Name;
    public Sprite sprite;
    public string Description;
}

