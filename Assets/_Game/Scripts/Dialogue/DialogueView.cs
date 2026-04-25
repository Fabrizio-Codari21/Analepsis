using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [SerializeField] private Transform m_root;
    
    public Action<string>  RecordRequested;

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
        var b = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(m_responseButton, Vector3.zero, Quaternion.identity, m_responseButtonRoot);
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
   
    public async UniTask UnfoldDialogue(bool isOpening)
    {
        if (m_root == null) return;
        Tween.StopAll(m_root.gameObject.transform);
        
        var seq = Sequence.Create();

        if (isOpening)
        {
            m_root.gameObject.transform.localScale = new Vector3(1, 0, 1);
            _ = seq.Group(Tween.ScaleY(m_root.gameObject.transform, 1f, 0.3f, Ease.OutBack));
        }
        else
        {
            _ = seq.Group(Tween.ScaleY(m_root.gameObject.transform, 0f, 0.2f, Ease.InQuad));
        }
        await seq;

    }
    
    
    public async UniTask PlayDialogueText(string content, CancellationToken token, Color color = default) // view  
    {
        token.ThrowIfCancellationRequested();
        var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dialogueTextSetting,
            Vector3.zero,
            Quaternion.identity,
            m_conversationRoot
        );
        t.SetText("- " + content,m_dialogueTextSetting.size, color != default ? color : playerDialogueColor);
        t.ToLast();
        await UniTask.NextFrame();
        token.ThrowIfCancellationRequested();
        m_scrollRect.verticalNormalizedPosition = 0;
        await t.PlayTypeWriterEffect(externalToken: token);
        var b = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(m_recordButton, Vector3.zero, Quaternion.identity, t.transform);
        b.SetFill(0f);
        b.AddListener(() =>
        {
            b.PlayImageFill(1f).Forget();
            RecordRequested?.Invoke(content);
           
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
    [SerializeField] private RecordNoteEvent m_recordNoteEvent;
    private IDialogable _dialoguer;

    private void Record(DialogueNode node)
    {
        
    }
}