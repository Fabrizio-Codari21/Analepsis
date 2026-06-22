using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Game/Npc",fileName = "NewNpc_0")]
public class NpcIdentity : ScriptableObject,IClue
{
    [Space(25), Header("CLUE DATA")]
    [Space(20)]
    public string npcName;
    [TextArea(0,30)] public string characterInfo;
    [PreviewField] public Sprite filePhoto;
    public SerializableGuid npcGuid = SerializableGuid.NewGuid();
    public Whodunnit possibleRoles;

    [Header("PERSONALITIES"),InfoBox("NOTE: None of these dictionaries should use 'None' as a Key.",Icon = SdfIconType.Newspaper)]

    [Space(15), Header("Pick a face for any emotion.")]
    public SerializedDictionary<Emotion, Sprite> allFaces = new();

    [Space(15), Header("Pick an animation for any reaction.")]
    [InfoBox("The values should correspond with a parameter in the animator.")]
    public SerializedDictionary<Reaction, string> allReactions = new();

    [Space(20)]
    public bool makesEyeContact = true;
    
    
    [Button("🔄 Actualizar Todos los GUIDs (Resolver Conflicto)", ButtonSizes.Large)]
    [InfoBox("Si has copiado este NPC mediante Ctrl+C / Ctrl+V, usa este botón para generar nuevos identificadores únicos para todos sus nodos y evitar problemas en el cuaderno.", InfoMessageType.Info)]
    public void RegenerateAllGuids()
    {
        
        npcGuid = SerializableGuid.NewGuid();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"<color=green>【GUID Actualizado Exitosamente】</color> Se han regenerado {npcName} GUIDs para el diálogo: <b>{name}</b>.");
    }

    public SerializableGuid CompareGuid()
    {
        return npcGuid;
    }
}


