using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
//using Unity.VisualScripting.Dependencies.Sqlite;

public class DialogueManager : PersistentSingleton<DialogueManager>,IActivity
{
    #region Event
 
    [SerializeField] private DialoguerEvent m_dialogueEvent;
    [SerializeField] private DialogueInputReader m_inputReader;
    [SerializeField] private RecordNoteEvent m_recordNoteEvent;
    #endregion
    [SerializeField] private DialogueView m_dialogueView;
    
    #region Screen
    [SerializeField] private IActivityEvent m_pushActivity;
    [SerializeField] private EventChannel m_popActivity;
    #endregion
    
    #region Cursor
    [SerializeField] private BoolEventChannel m_cursorEnable;
    #endregion
   
    #region Setting
    [SerializeField] private float timeToOutDialogue; // se usa cuando la última palabra se la da el npc
    [ShowInInspector, ReadOnly] private HashSet<SerializableGuid> _dialogueNodesTalked = new();
    private IDialogable _currentDialoguer;
    private CancellationTokenSource  _dialogueCts;
    private string _recordText = string.Empty;
    private string _manualRecords = string.Empty;
    private string _topic = string.Empty;
    #endregion

    #region Data

    private HashSet<int> _recordedPathHashes = new();
    private int _currentPathHash = 17; // PRIMER NUMERO PRIMO
    
    #endregion
    
    #region  IActivity
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    public void Resume()
    {
        OnResume?.Invoke();
        m_inputReader?.SetEnable();
        m_dialogueView?.gameObject.SetActive(true);
        if (m_inputReader) m_inputReader.Skip += Skip;
        m_cursorEnable.Raise(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_inputReader?.SetEnable(false);
        m_dialogueView?.gameObject.SetActive(false);
        if (m_inputReader) m_inputReader.Skip -= Skip;
        m_cursorEnable.Raise(false);
    }

    public void Stop()
    {
       OnStop?.Invoke();
       Pause();
    }


    private void Skip()
    {
        _dialogueCts?.Cancel();
    }

    public bool CanPopWithKey()
    {
        return false;
    }
    #endregion
    private void Start()
    {
        m_dialogueView = Instantiate(m_dialogueView,transform);
        
        m_dialogueView.RecordRequested += content =>
        {
            if (_currentDialoguer == null) return;
            AppendToText(ref _manualRecords, content);
            
        };
        m_dialogueEvent.OnEventRaised += dialogable => _ = Speak(dialogable);
    }
    private async UniTaskVoid Speak(IDialogable dialogable)   
    {
        m_pushActivity.Raise(this);
        _currentDialoguer = dialogable;
        _currentDialoguer.Dialogue._hiddenProof.Clear();
        m_dialogueView.ClearDialogues();
        m_dialogueView.SetSpeakerName(dialogable.NpcName);
        await m_dialogueView.UnfoldDialogue(true);
        await PlayDialogueNode(dialogable.Dialogue.startingNode);
    }
    
    private async UniTask PlayDialogueNode(DialogueNode node) 
    {
        if(node == null) return;

        unchecked 
        {
            _currentPathHash = _currentPathHash * 31 + node.guid.GetHashCode();
        }
        _dialogueNodesTalked.Add(node.guid);
        ResetCancellationToken();
        var token = _dialogueCts.Token;
    
        m_dialogueView.ClearResponses();
        if (node.doesItProveAnything != 0) _currentDialoguer.Dialogue.DiscoverProof(node.doesItProveAnything);
        
        AppendToRecord(node.dialogueText);
        try 
        {
            if(node.characterEmotion != Emotion.None) _currentDialoguer.SetFace(node.characterEmotion);

            await m_dialogueView.PlayDialogueText(node.dialogueText, token, _currentDialoguer.Dialogue.dialogueColor);
        }
        catch (OperationCanceledException) 
        {
    
        }

        if (_currentDialoguer == null) return;

        List<DialogueResponse> availableResponses = node.responses?.FindAll(res => res.IsAvailable()) ?? new List<DialogueResponse>();
    
        if (availableResponses.Count == 0)
        {
            try 
            {
                await UniTask.Delay(TimeSpan.FromSeconds(timeToOutDialogue), cancellationToken: token);
            }
            catch (OperationCanceledException) { }

            EndDialogue(); 
            return;
        }
        
        foreach (var response in availableResponses)
        {
            string tagToDisplay = String.Empty;
            if (response.IsNewResponse())
            {
                tagToDisplay = "New";
            }
            else if (response.ShouldShowNewPath())
            {
                tagToDisplay = "HAS NEW RESPONSE IN THIS PATH";
            }
            
            var button = m_dialogueView.CreateResponseButton(response.responseText,tagToDisplay);
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
        if (response.nextNode == null)
        {
            await m_dialogueView.UnfoldDialogue(false);
            EndDialogue(response.HasTopic(out _topic));
            return;
        }
        AppendToRecord($"[Player]: {response.responseText}");
    
        try 
        {
            await m_dialogueView.PlayDialogueText(response.responseText, token);
        }
        catch (OperationCanceledException) { }

        if (_currentDialoguer == null) return;
    
        try 
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        }
        catch (OperationCanceledException) { }

        if (response.nextNode != null)
        {
            PlayDialogueNode(response.nextNode).Forget();
        }
        else
        {
            await m_dialogueView.UnfoldDialogue(false);
            EndDialogue(response.HasTopic(out _topic));
        }
    }
    
    private void AppendToText(ref string target, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (target != string.Empty) target += "\n\n";
        target += "- " + text;
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

    private void EndDialogue(bool withTopic = false)
    {
      
        if (!_recordedPathHashes.Contains(_currentPathHash))
        {
            string title = $"{_currentDialoguer.NpcName}'s account" + (withTopic ? $" -\n About {_topic.ToLower()}" : " -\n No clear topic");
            var log = new LogNote(title, _recordText, _manualRecords, _currentDialoguer.ID);
            m_recordNoteEvent.Raise(log);
            _recordedPathHashes.Add(_currentPathHash);
        }
        
        _currentPathHash =  17;
        _currentDialoguer.SetFace(_currentDialoguer.DefaultEmotion);
        _currentDialoguer = null;
        _recordText = string.Empty;
        _manualRecords = string.Empty;
        m_popActivity.Raise();
        
    }
    
    
    public bool CheckDialogue(SerializableGuid guid) => _dialogueNodesTalked.Contains(guid);
}

