using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

using UnityEngine;

// Objeto base que contiene toda la logica de un dialogo.
[Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue Assets/New Dialogue")]
public class Dialogue :SerializedScriptableObject
{
    [Space(25), Header("CLUE DATA")]
    [Space(20)]
    [Header("DIALOGUE")]
    public Color dialogueColor;
    public DialogueNode startingNode;
    public List<DialogueNode> allNodes =  new List<DialogueNode>();

    [ReadOnly] public List<Whodunnit> hiddenProof = new();
    public void DiscoverProof(Whodunnit proof)
    {
        if (hiddenProof.Contains(proof)) return;
        hiddenProof.Add(proof);
    }
    
    
    [Button("🔄 Actualizar Todos los GUIDs (Resolver Conflicto)", ButtonSizes.Large)]
    [InfoBox("Si has copiado este diálogo mediante Ctrl+C / Ctrl+V, usa este botón para generar nuevos identificadores únicos para todos sus nodos y evitar problemas en el cuaderno.", InfoMessageType.Info)]
    public void RegenerateAllGuids()
    {
        int updatedCount = 0;

       
        if (startingNode != null)
        {
            startingNode.guid = SerializableGuid.NewGuid();
            updatedCount++;
            
        }

   
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                node.guid = SerializableGuid.NewGuid();
                updatedCount++;
                
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"<color=green>【GUID Actualizado Exitosamente】</color> Se han regenerado {updatedCount} GUIDs para el diálogo: <b>{name}</b>.");
    }
}




