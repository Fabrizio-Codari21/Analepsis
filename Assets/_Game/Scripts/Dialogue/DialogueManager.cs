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
        // estos clear token
        _dialogueNodesTalked.Add(node.guid);
        _dialogueCts?.Cancel();
        _dialogueCts?.Dispose();
        _dialogueCts = new CancellationTokenSource();
        var token = _dialogueCts.Token;
        
        // clear los botones de respose
        m_dialogueView.ClearResponses();
        await m_dialogueView.PlayDialogueText(node, token);
        
        // esto recuerada para anotar en notebook
        if(_recordText != string.Empty) _recordText += "\n\n";
        _recordText += "- " + $"{node.dialogueText}";
        
        if(token.IsCancellationRequested) return;
        
        // si no hay respuesta disponible
        if (node.responses == null || node.responses.Count == 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(timeToOutDialogue), cancellationToken: token);
            if (token.IsCancellationRequested) return;
            EndDialogue(); return;
        }
        
        // calculo cuales son la respuesta disponible acutar
        List<DialogueResponse> availableResponses = new List<DialogueResponse>();
        if (node.responses != null)
        {
            foreach (var res in node.responses)
            {
                if (res.IsAvailable()) availableResponses.Add(res);
            }
        }
        
        
        // si no hay ninguna termina tmb
        if (availableResponses.Count == 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(timeToOutDialogue), cancellationToken: token);
            if (token.IsCancellationRequested) return;
            EndDialogue(); 
            return;
        }
        
        // foreach de la respuesta y bla bla
        foreach (var response in availableResponses)
        {
            var nextNode = response.nextNode;
            var button = m_dialogueView.CreateResponseButton(response.responseText);
            button.AddListener(
                    () => {
                    button.SetInteractable(false);
                    m_dialogueView.ClearResponses();
                    if (nextNode != null)
                    {
                        _ = PlayDialogueNode(nextNode);
                    }
                    else
                    {
                        EndDialogue();
                    } 
                    }
                );
        }
    
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