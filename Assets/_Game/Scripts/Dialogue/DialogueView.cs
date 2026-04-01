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
    
    

    private async UniTaskVoid AddDialogueAsync(IDialogable dialogable)
    {
        m_dialoguerName.text = dialogable.Name;
        var t = FlyweightFactory.Instance.Spawn<DynamicText>( m_dialogueTextSetting, Vector3.zero, Quaternion.identity, m_conversationRoot);
        t.SetText(dialogable.Dialogue.startingNode.dialogueText);
        await UniTask.Yield();
        m_scrollRect.verticalNormalizedPosition = 0;
        await t.PlayTypeWriterEffect();
     
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