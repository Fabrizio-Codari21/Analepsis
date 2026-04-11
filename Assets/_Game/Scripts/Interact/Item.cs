using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Item",fileName = "Item")]
public class Item : ScriptableObject, IClue
{
    public GameObject gameObject;
    
    public SerializableGuid guid = SerializableGuid.NewGuid();
    [MinValue(1)] public float size = 1;
    public string Name;
    public Sprite sprite;
    public string Description;

    [SerializeField] List<TheoryboardManager.Whodunnit> doesItProveAnything;

    public List<TheoryboardManager.Whodunnit> DoesItProveAnything()
    {
        return doesItProveAnything;
    }
}

public interface IClue
{
    public List<TheoryboardManager.Whodunnit> DoesItProveAnything();
}

