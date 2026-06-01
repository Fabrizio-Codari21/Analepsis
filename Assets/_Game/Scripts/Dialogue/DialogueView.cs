using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using System.Linq;

public class DialogueView : MonoBehaviour
{
    [Header("Name")]
    [SerializeField] private TMP_Text m_dialoguerName; 
    [Header("Dynamic Text")]
    [SerializeField] private DynamicTextSetting m_dialogueTextSetting;
    [SerializeField] private Transform m_conversationRoot;
    [SerializeField] Color playerDialogueColor;
    [Header("Response Button")]
    [SerializeField] private ButtonSetting m_responseButton;
    [SerializeField] private Transform m_responseButtonRoot;
    
    [SerializeField] private ButtonSetting m_recordButton;
    [Header("Scroll")]
    [SerializeField] private ScrollRect m_scrollRect;
    
    [SerializeField] private Check m_checkEvidence;
    [SerializeField] private EvidenceEvent m_evidenceEvent;
    [SerializeField] private Transform m_root;
    

    private void Start()
    {
        gameObject.SetActive(false);
    }
    
    public void SetSpeakerName(string newName)
    {
        m_dialoguerName.text = newName;
    }
    
    public ButtonFactoryObject CreateResponseButton(string text)
    {
        var b = FlyweightFactory.Instance.Spawn<TagButton>(m_responseButton, Vector3.zero, Quaternion.identity, m_responseButtonRoot);
        b.SetText(text);
        b.SetInteractable(true);
        b.MoveToLast();
        return b;
    }
    public void ClearResponses()
    {
        Despawn(m_responseButtonRoot);
    }

    public void ClearDialogues()
    {
        Despawn(m_conversationRoot);
    }
   
    public async UniTask UnfoldDialogue(bool isOpening, bool makesEyeContact = true, MultiAimConstraint npc = default, MultiAimConstraint player = default)
    {
        if (m_root == null) return;
        Tween.StopAll(m_root.gameObject.transform);

        var scale = UIManager.Instance.AspectRatioScale(0.2f);
        var seq = Sequence.Create();
        if (isOpening)
        {
            m_root.gameObject.transform.localScale = new Vector3(1, 0, 1) * scale;
            _ = seq.Group(Tween.ScaleY(m_root.gameObject.transform, scale, 0.3f, Ease.OutBack));
            if (npc != default && player != default && makesEyeContact) _ = MakeEyeContact(npc, player, 1, 0.9f); 
        }
        else
        {
            _ = seq.Group(Tween.ScaleY(m_root.gameObject.transform, 0f, 0.2f, Ease.InQuad));
            if (npc != default && player != default && makesEyeContact) _ = MakeEyeContact(npc, player, 1, 0);

        }
        await seq;

    }

    // por ahora el npc gira bien pero el player no, lo voy a seguir revisando.
    private async UniTask MakeEyeContact(MultiAimConstraint npc, MultiAimConstraint player = default, float time = 1f, float maxWeight = 1f)
    {
        WeightedTransform npcPosition = new(npc.transform, 1f);
        if(maxWeight > 0f)
        {
            _ = Tween.Custom(npc.weight, maxWeight, time, (x => npc.weight = x), Ease.OutCirc);
            if(player != default)
            {
                player.data.sourceObjects.Add(npcPosition);
                //print(player.data.sourceObjects.Count);
                await Tween.Custom(player.weight, maxWeight, time, (x => player.weight = x), Ease.OutCirc);

            }

            //while (looker.weight < maxWeight) {looker.weight += 0.02f / time; await UniTask.Delay(20);}
        }
        else
        {
            _ = Tween.Custom(npc.weight, 0, time, (x => npc.weight = x), Ease.OutCirc);
            if (player != null)
            {
                await Tween.Custom(player.weight, 0, time, (x => player.weight = x), Ease.OutCirc);
                player.data.sourceObjects.Remove(npcPosition);
            }
            
        }
    }
  
    public async UniTask PlayDialogueText(string content, CancellationToken token, Color color = default, bool isResponse = false) // view  
    {
        token.ThrowIfCancellationRequested();

        var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dialogueTextSetting,
            Vector3.zero,
            Quaternion.identity,
            m_conversationRoot);

        t.SetText("- " + content,m_dialogueTextSetting.size, color != default ? color : playerDialogueColor, compensateLines: 1);
        t.ToLast();
        await UniTask.NextFrame();
        token.ThrowIfCancellationRequested();
        m_scrollRect.verticalNormalizedPosition = 0;
        await t.PlayTypeWriterEffect(externalToken: token);

        if (isResponse) return; //por ahora hago que las respuestas del player no se puedan resaltar.

        var b = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            m_recordButton, 
            Vector3.zero, 
            Quaternion.identity, 
            t.transform);

        if (b.TryGetComponent<ImageSelector>(out var select)) select.SetSprite((int)(t.GetSize().y / m_dialogueTextSetting.size));
        

        b.AddListener(() =>
        {            
                    
        });
        
        
    }
    
    public async UniTask PlayNpcDialogue(DialogueNode node, CancellationToken token, Color color = default, bool isResponse = false) // view  
    {
        token.ThrowIfCancellationRequested();

        var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dialogueTextSetting,
            Vector3.zero,
            Quaternion.identity,
            m_conversationRoot);

        t.SetText("- " + node.dialogueText,m_dialogueTextSetting.size, color != default ? color : playerDialogueColor, compensateLines: 1);
        t.ToLast();
        await UniTask.NextFrame();
        token.ThrowIfCancellationRequested();
        m_scrollRect.verticalNormalizedPosition = 0;
        await t.PlayTypeWriterEffect(externalToken: token);

        if (isResponse) return; //por ahora hago que las respuestas del player no se puedan resaltar.

        var b = FlyweightFactory.Instance.Spawn<FillMarkButton>(
            m_recordButton, 
            Vector3.zero, 
            Quaternion.identity, 
            t.transform);

        if (b.TryGetComponent<ImageSelector>(out var select)) select.SetSprite((int)(t.GetSize().y / m_dialogueTextSetting.size));


        var contain = m_checkEvidence.Request(node.guid);
        select.SetFill(contain ? 1 : 0);

        b.AddListener(() =>
        {
            var has = m_checkEvidence.Request(node.guid);
            
            string defaultName = node.PreviousResponse != null ? node.PreviousResponse.responseText : "Beginning";

            var fragmentEvidenceToMark = EvidenceDataBase.Instance.GetOrCreate(node.guid, () => new DialogueFragmentNote(defaultName, node.guid, node.doesItProveAnything, node));
            if (has)
            {
                b.PlayImageFill(0).Forget();
                m_evidenceEvent.Raise(fragmentEvidenceToMark);
            }
            else
            {
                b.PlayImageFill(1).Forget();
                m_evidenceEvent.Raise(fragmentEvidenceToMark);
            }
        });
        
        
    }
   
    
    
    private void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }
    
}

public class DialogueMarkClueButton : MonoBehaviour
{
    [SerializeField] private ButtonSetting m_button;
    [SerializeField] private NoteEvent noteEvent;
    private IDialogable _dialoguer;

    private void Record(DialogueNode node)
    {
        
    }
}