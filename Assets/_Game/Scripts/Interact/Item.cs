using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject
{
    public GameObject gameObject;

    [MinValue(1)] public float size;
}