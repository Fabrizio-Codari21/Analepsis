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
    public List<Whodunnit> possibleRoles;

    [Header("PERSONALITIES"),InfoBox("NOTE: None of these dictionaries should use 'None' as a Key.",Icon = SdfIconType.Newspaper)]

    [Space(15), Header("Pick a face for any emotion.")]
    public SerializedDictionary<Emotion, Sprite> allFaces = new();

    [Space(15), Header("Pick an animation for any reaction.")]
    [InfoBox("The values should correspond with a parameter in the animator.")]
    public SerializedDictionary<Reaction, string> allReactions = new();

    [Space(20)]
    public bool makesEyeContact = true;

}


