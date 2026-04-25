using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(menuName = "Game/Npc",fileName = "NewNpc_0")]
public class NpcIdentity : ScriptableObject
{
    public string npcName;
    public Whodunnit role;
    
    public SerializableGuid  npcGuid = SerializableGuid.NewGuid();
    public SerializedDictionary<Emotion, Sprite> allFaces = new();

}



