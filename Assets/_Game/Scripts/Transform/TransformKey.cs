using UnityEngine;

[CreateAssetMenu(menuName = "Game/Key/Transform",fileName = "TransformKey")]
public class TransformKey : ScriptableObject
{
    [SerializeField] private string description;
}