using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class CutsceneInfo
{
    [Header("CHARACTER INFO")]
    public NpcIdentity leftCharacter;
    public NpcIdentity rightCharacter;
    //public SerializedDictionary<Emotion, Sprite> leftCharacter;
    //public SerializedDictionary<Emotion, Sprite> rightCharacter;
    [Space(10)] public bool startWithTheRightCharacter = false;

    [Space(20), Header("DIALOGUE INFO")]
    public string dialogueID;
    public List<SimpleDialogueNode> dialogue;

}

[System.Serializable]
public class SimpleDialogueNode
{
    [TextArea(0, 20)] public string dialogue;
    public Emotion emotion = Emotion.Idle;
    public bool switchCharacter = true;
}
