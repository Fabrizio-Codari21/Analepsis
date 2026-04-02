using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour, IActivity
{
    [SerializeField] private DialogueInputReader m_inputReader;
    [SerializeField] private TMP_Text m_dialoguerName; // Nombre del personaje y contenido del dialogo
    [SerializeField] private DynamicTextSetting m_dialogueTextSetting;
    [SerializeField] private Transform m_conversationRoot;

    [SerializeField] private ScrollRect m_scrollRect;

    [SerializeField] private ButtonSetting m_responseButton;
    [SerializeField] private Transform m_responseButtonRoot;

    [SerializeField] private DialoguerEvent m_dialogueEvent;
    private CancellationTokenSource _dialogueCts;
    private void Start()
    {
        m_dialogueEvent.OnEventRaised += dialogue => _ = Speck(dialogue);
    }

    #region  IActivity

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public void Resume()
    {
        OnResume?.Invoke();
        m_inputReader.SetEnable();
        gameObject.SetActive(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_inputReader.SetEnable(false);
        gameObject.SetActive(false);
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }

    public bool CanPopWithKey()
    {
        return false;
    }
    
    #endregion


    private async UniTaskVoid Speck(IDialogable dialogable)
    {
        await AddDialogueAsync(dialogable);
    }

    private async UniTask AddDialogueAsync(IDialogable dialogable)
    {
        m_dialoguerName.text = dialogable.Name;
        await PlayDialogueNode(dialogable.Dialogue.startingNode);
    }


    private async UniTask PlayDialogueNode(DialogueNode node)
    {
        if(node == null) return;
        _dialogueCts?.Cancel();
        _dialogueCts?.Dispose();
        _dialogueCts = new CancellationTokenSource();

        var token = _dialogueCts.Token;
        try
        {
            Despawn(m_responseButtonRoot);

            await PlayDialogueText(node, token);

            if (token.IsCancellationRequested) return;

            AddDialogueResponseButton(node);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask PlayDialogueText(DialogueNode dialogueNode, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var t = FlyweightFactory.Instance.Spawn<DynamicText>(
            m_dialogueTextSetting,
            Vector3.zero,
            Quaternion.identity,
            m_conversationRoot
        );

        string content = dialogueNode.dialogueText;
        t.SetText("- " + content);
        await UniTask.NextFrame();
        token.ThrowIfCancellationRequested();
        m_scrollRect.verticalNormalizedPosition = 0;
        await t.PlayTypeWriterEffect(externalToken: token);
    }

    private void AddDialogueResponseButton(DialogueNode node)
    {
        foreach (var response in node.responses)
        {
            var next = response.nextNode;
            var b = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>( m_responseButton, Vector3.zero, Quaternion.identity, m_responseButtonRoot);
            b.SetInteractable(true);
            b.SetText(response.responseText);
            if (next != null)
            {
                b.AddListener(() =>
                {
                    b.SetInteractable(false);
                    Despawn(m_responseButtonRoot);
                    _ = PlayDialogueNode(next);
                });
            }
        }
    }

    private void Despawn(Transform root)
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }
    
   
}

public interface IDialogable : IInteractable
{
    public string Name { get; set; }
    public Dialogue Dialogue { get; }
    public Dialogue NewDialogue(Dialogue dialogue);

}

public interface INpc : IDialogable
{
    public NpcIdentity ID { get; set; }
}