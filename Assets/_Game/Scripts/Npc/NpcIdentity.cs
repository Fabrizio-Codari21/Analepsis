using UnityEngine;

[CreateAssetMenu(menuName = "Game/Npc",fileName = "NewNpc_0")]
public class NpcIdentity : ScriptableObject
{
    public string npcName;
    
    public SerializableGuid  npcGuid = SerializableGuid.NewGuid();
    
    
}
