using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI INFO")]
    public GameObject cutsceneUI;

    public Image leftCharacter, rightCharacter;
    public TextMeshProUGUI characterName; 
    public DynamicUIText dialogueText;
    public Button nextButton;

    CutsceneTrigger _currentTrigger;
    public void SetCutscene(CutsceneTrigger currentTrigger) => _currentTrigger = currentTrigger;
    public void DisableCutscene()
    {
        cutsceneUI.SetActive(false);
        if (!_currentTrigger) return;
        _currentTrigger.Stop();
        _currentTrigger = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cutsceneUI.SetActive(false);
    }

    public async UniTask PlayDialogue(CutsceneInfo cutscene, bool isLeftCharacter = true, int currentNode = 0)
    {
        var node = cutscene.dialogue[currentNode];

        nextButton.onClick.RemoveAllListeners();

        if (!cutsceneUI.activeInHierarchy) cutsceneUI.SetActive(true);
        leftCharacter.sprite = cutscene.leftCharacter.cutsceneSprites[Emotion.Idle];
        rightCharacter.sprite = cutscene.rightCharacter.cutsceneSprites[Emotion.Idle];

        characterName.text = isLeftCharacter ? cutscene.leftCharacter.npcName : cutscene.rightCharacter.npcName;
        if (isLeftCharacter)
        {
            leftCharacter.sprite = SetEmotion(cutscene.leftCharacter, node.emotion) ?? leftCharacter.sprite;
            leftCharacter.color += new Color(0,0,0,1);
            rightCharacter.color = new Color(rightCharacter.color.r, rightCharacter.color.g, rightCharacter.color.b, 0.4f);
        }
        else 
        {
            rightCharacter.sprite = SetEmotion(cutscene.rightCharacter, node.emotion) ?? rightCharacter.sprite;
            rightCharacter.color += new Color(0, 0, 0, 1);
            leftCharacter.color = new Color(leftCharacter.color.r, leftCharacter.color.g, leftCharacter.color.b, 0.4f);
        }

        dialogueText.SetText(node.dialogue, 1, Color.black);
        //print($"{(isLeftCharacter ? "Left Character" : "Right Character")}");
        await dialogueText.PlayTypeWriterEffect();
        //character = !character;

        if(cutscene.dialogue.Count > currentNode + 1)
        nextButton.onClick.AddListener(() => _ = PlayDialogue(cutscene, node.switchCharacter ? !isLeftCharacter : isLeftCharacter, currentNode + 1));
        else nextButton.onClick.AddListener(() => DisableCutscene());

    }

    public Sprite SetEmotion(NpcIdentity character, Emotion id)
    {
        if (character.cutsceneSprites.ContainsKey(id)) return character.cutsceneSprites[id];
        else if (character.cutsceneSprites.ContainsKey(Emotion.Idle)) return character.cutsceneSprites[Emotion.Idle];
        else return null;
    }

    //public UniTask PlayDialogue(CutsceneInfo cutscene)
    //{
    //    bool character = cutscene.startWithTheRightCharacter ? false : true;
    //    foreach (var node in cutscene.dialogue)
    //    {
    //        var c = character ? cutscene.leftCharacter : cutscene.rightCharacter;
    //        var index = cutscene.dialogue.IndexOf(node);

    //        print($"Node {index + 1}: [{node.emotion}; {c[node.emotion]}] - {node.dialogue}");

    //        character = !character;
    //        UniTask.Delay(100);
    //    }
    //    return UniTask.Delay(100);
    //}
}
