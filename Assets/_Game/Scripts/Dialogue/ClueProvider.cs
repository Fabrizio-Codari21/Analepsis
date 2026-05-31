#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;

public static class ClueProvider
{
    public static IEnumerable<ValueDropdownItem<IClue>> GetAvailableClues()
    {
        var dropdownList = new List<ValueDropdownItem<IClue>>();
  
        string[] itemGuids = AssetDatabase.FindAssets("t:Item");
        foreach (var guid in itemGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(path);
            if (item == null) continue;

            dropdownList.Add(new ValueDropdownItem<IClue>($"[1_Item] / {item.Name} ({item.name})", item));
        }
        

        string[] npcGuids = AssetDatabase.FindAssets("t:NpcIdentity"); 
        foreach (var guid in npcGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            NpcIdentity npc = AssetDatabase.LoadAssetAtPath<NpcIdentity>(path);
            if (npc == null) continue;

           
            dropdownList.Add(new ValueDropdownItem<IClue>($"[2_NPC] / {npc.npcName} ({npc.name})", npc));
        }

   
        string[] dialogueGuids = AssetDatabase.FindAssets("t:Dialogue");
        foreach (var guid in dialogueGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Dialogue dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(path);
            if (dialogue == null || dialogue.allNodes == null) continue;

            foreach (var node in dialogue.allNodes)
            {
                if (node == null) continue;
                
                string nodeSummary = string.IsNullOrEmpty(node.tag) 
                    ? (node.dialogueText.Length > 15 ? node.dialogueText.Substring(0, 15) + "..." : node.dialogueText) 
                    : node.tag;
                
                string pathName = $"[3_Dialogue] / {dialogue.name} / [{nodeSummary}]";
                
                dropdownList.Add(new ValueDropdownItem<IClue>(pathName, node));
            }
        }

        return dropdownList;
    }
}
#endif