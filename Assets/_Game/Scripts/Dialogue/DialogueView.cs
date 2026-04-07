using System.Threading;
using Cysharp.Threading.Tasks;
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
    [Header("Response Button")]
    [SerializeField] private ButtonSetting m_responseButton;
    [SerializeField] private Transform m_responseButtonRoot;
    [Header("Scroll")]
    [SerializeField] private ScrollRect m_scrollRect;

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
   
    public async UniTask PlayDialogueText(DialogueNode dialogueNode, CancellationToken token) // view
    {
        token.ThrowIfCancellationRequested();

        var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dialogueTextSetting,
            Vector3.zero,
            Quaternion.identity,
            m_conversationRoot
        );
        string content = dialogueNode.dialogueText;
        t.SetText("- " + content,m_dialogueTextSetting.size,m_dialogueTextSetting.color);
        await UniTask.NextFrame();
        token.ThrowIfCancellationRequested();
        m_scrollRect.verticalNormalizedPosition = 0;
        await t.PlayTypeWriterEffect(externalToken: token);
    }

   

    private void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }
    

    
   
}