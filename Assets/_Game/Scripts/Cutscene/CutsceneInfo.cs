using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class CutsceneInfo
{
    [Header("CHARACTER INFO")]
    public NpcIdentity leftCharacter, rightCharacter;
    //public SerializedDictionary<Emotion, Sprite> leftCharacter;
    //public SerializedDictionary<Emotion, Sprite> rightCharacter;
    [Space(10)] public bool startWithTheRightCharacter = false;

    [Space(20), Header("DIALOGUE INFO")]
    public List<SimpleDialogueNode> dialogue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

[System.Serializable]
public class SimpleDialogueNode
{
    [TextArea(0, 20)] public string dialogue;
    public Emotion emotion = Emotion.Idle;
    public bool switchCharacter = true;
}
