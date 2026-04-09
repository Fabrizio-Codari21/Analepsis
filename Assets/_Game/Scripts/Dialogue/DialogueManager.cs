using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public class DialogueManager : PersistentSingleton<DialogueManager>,IActivity
{
    [SerializeField] private DialoguerEvent m_dialogueEvent;
    [SerializeField] private DialogueInputReader m_inputReader;
    [SerializeField] private DialogueView m_dialogueView;
    private CancellationTokenSource  _dialogueCts;

    [SerializeField] private IActivityEvent m_pushActivity;
    [SerializeField] private EventChannel m_popActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;
    [SerializeField] private float timeToOutDialogue; // se usa cuando la última palabra se la da el npc
    [SerializeField] private RecordNoteEvent m_recordNoteEvent;
    private IDialogable _currentDialoguer;
    private string _recordText = string.Empty;
    
    
    [ShowInInspector, ReadOnly] private HashSet<SerializableGuid> _dialogueNodesTalked = new HashSet<SerializableGuid>();
    #region  IActivity
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    public void Resume()
    {
        OnResume?.Invoke();
        m_inputReader?.SetEnable();
        m_dialogueView?.gameObject.SetActive(true);
        m_cursorEnable.Raise(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_inputReader?.SetEnable(false);
        m_dialogueView?.gameObject.SetActive(false);
        m_cursorEnable.Raise(false);
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
    private void Start()
    {
        m_dialogueView = Instantiate(m_dialogueView,transform);
        m_dialogueEvent.OnEventRaised += dialogable => _ = Speak(dialogable);
    }
    private async UniTaskVoid Speak(IDialogable dialogable)   
    {
        m_pushActivity.Raise(this);
        _currentDialoguer = dialogable;
        m_dialogueView.ClearDialogues();
        m_dialogueView.SetSpeakerName(dialogable.NpcName);
        await PlayDialogueNode(dialogable.Dialogue.startingNode);
    }
    
    private async UniTask PlayDialogueNode(DialogueNode node) 
    {
        if(node == null) return;

        _dialogueNodesTalked.Add(node.guid);
        ResetCancellationToken();
        var token = _dialogueCts.Token;
        
        m_dialogueView.ClearResponses();
        
        AppendToRecord(node.dialogueText);
        await m_dialogueView.PlayDialogueText(node.dialogueText, token);
        
        if(token.IsCancellationRequested) return;

  
        List<DialogueResponse> availableResponses = node.responses?.FindAll(res => res.IsAvailable()) ?? new List<DialogueResponse>();
        
        if (availableResponses.Count == 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(timeToOutDialogue), cancellationToken: token);
            if (token.IsCancellationRequested) return;
            EndDialogue(); 
            return;
        }
        
     
        foreach (var response in availableResponses)
        {
            var button = m_dialogueView.CreateResponseButton(response.responseText);
            button.AddListener(() => {
                button.SetInteractable(false);
                PlayResponseProcess(response).Forget();
            });
        }
    }

    private async UniTaskVoid PlayResponseProcess(DialogueResponse response)
    {
        ResetCancellationToken();
        var token = _dialogueCts.Token;

        m_dialogueView.ClearResponses();
        
       
        AppendToRecord($"[Player]: {response.responseText}");
        
        await m_dialogueView.PlayDialogueText(response.responseText, token);

        if (token.IsCancellationRequested) return;
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);

        if (response.nextNode != null)
        {
            PlayDialogueNode(response.nextNode).Forget();
        }
        else
        {
            EndDialogue();
        }
    }


    private void AppendToRecord(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (_recordText != string.Empty) _recordText += "\n\n";
        _recordText += "- " + text;
    }
    private void ResetCancellationToken()
    {
        _dialogueCts?.Cancel();
        _dialogueCts?.Dispose();
        _dialogueCts = new CancellationTokenSource();
    }


    private void EndDialogue()
    {
        m_recordNoteEvent.Raise(new LogNote(_currentDialoguer.NpcName,_recordText));
        _currentDialoguer = null;
        _recordText = String.Empty;
        m_popActivity.Raise();
        
    }
    
    
    public bool CheckDialogue(SerializableGuid guid) => _dialogueNodesTalked.Contains(guid);
}