using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
//using Unity.VisualScripting.Dependencies.Sqlite;

public class DialogueManager : PersistentSingleton<DialogueManager>,IActivity
{
    [SerializeField] private DialoguerEvent m_dialogueEvent;
    [SerializeField] private DialogueInputReader m_inputReader;
    [SerializeField] private DialogueView m_dialogueView;
    [SerializeField] private Transform m_player;

    [SerializeField] private IActivityEvent m_pushActivity;
    [SerializeField] private EventChannel m_popActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;
    [SerializeField] private float timeToOutDialogue; // se usa cuando la última palabra se la da el npc
    [SerializeField] private RecordNoteEvent m_recordNoteEvent;
    private CancellationTokenSource  _dialogueCts;
    private IDialogable _currentDialoguer;
    
    
    private HashSet<string> _recordedContentInSession = new HashSet<string>();

    private List<string> _manualRecords = new();
    private List<string> _previousRecords = new();
    private List<string> _recordText = new();
    private string _topic = string.Empty;
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
        m_dialogueView.m_player = m_player;
        
        m_dialogueView.RecordRequested += (content, button) =>
        {
            if (_currentDialoguer == null) return;
            if (!_recordedContentInSession.Add(content) || _previousRecords.Contains("- " + content)) 
            {
                button.PlayImageFill(0f).Forget();
                RemoveFromText(ref _manualRecords, content);
                _recordedContentInSession.Remove(content);
                _previousRecords.Remove("- " + content);
                return; 
            }

            button.PlayImageFill(1f, color: new(0.1f,0,0.4f,1)).Forget();
            AppendToText(ref _manualRecords, content);
            
        };
        m_dialogueView.IsAlreadyRecorded += (content) =>
        {
            return _previousRecords.Contains("- " + content);
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
        await m_dialogueView.UnfoldDialogue(
            true, 
            _currentDialoguer.ID.makesEyeContact,
            _currentDialoguer.LookAt, 
            _currentDialoguer.Player);
        await PlayDialogueNode(dialogable.Dialogue.startingNode);

    }


    private async UniTask PlayDialogueNode(DialogueNode node) 
    {
        if(node == null) return;

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
            // esto lo descomentamos si queremos que solo se vean la primera vez, igual no funciona todavia.
            //if (_currentDialoguer.FirstTimeSpeaking) response.alreadyDisplayed = false;
            string tagToDisplay = string.Empty;
            bool wasUnlocked = false;
            if (NotebookManager.Instance.FoundCharacters.ContainsKey(_currentDialoguer.ID))
            {
                if (response.IsNewResponse())
                {
                    //tagToDisplay = "NEW";
                    wasUnlocked = true;
                    print(wasUnlocked);
                    //response.alreadyDisplayed = true;
                }
                else if (response.ShouldShowNewPath())
                {
                    //tagToDisplay = "PATH EXPANDED";
                    //response.alreadyDisplayed = true;
                }
                //print(tagToDisplay);
            }
            //else if (response.IsNewResponse()) response.alreadyDisplayed = false;

            ResponseDialogueButton button = 
                (ResponseDialogueButton)m_dialogueView.CreateResponseButton(response.responseText, tagToDisplay);

            button.AddListener(() => {
                button.SetInteractable(false);
                PlayResponseProcess(response).Forget();
            });
            button.MarkAsLinked(wasUnlocked);
        }
        await UniTask.NextFrame();
    }

    private async UniTaskVoid PlayResponseProcess(DialogueResponse response)
    {
        ResetCancellationToken();
        var token = _dialogueCts.Token;

        m_dialogueView.ClearResponses();
        if (response.nextNode == null)
        {
            await m_dialogueView.UnfoldDialogue(
                false, 
                _currentDialoguer.ID.makesEyeContact,
                _currentDialoguer.LookAt, 
                _currentDialoguer.Player);

            EndDialogue(response.HasTopic(out _topic));
            return;
        }
        AppendToRecord($"[Player]: {response.responseText}");
    
        try 
        {
            await m_dialogueView.PlayDialogueText(response.responseText, token, isResponse: true);
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
            await m_dialogueView.UnfoldDialogue(
                           false,
                           _currentDialoguer.ID.makesEyeContact,
                           _currentDialoguer.LookAt,
                           _currentDialoguer.Player);

            EndDialogue(response.HasTopic(out _topic));
        }
    }
    
    bool _recordChanged = false;
    private void AppendToText(ref List<string> target, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        target.Add("- " + text);
        _recordChanged = true;
    }

    private void RemoveFromText(ref List<string> target, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        target.Remove("- " + text);
        _recordChanged = true;
    }

    private void AppendToRecord(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        _recordText.Add("- " + text);
    }
    private void ResetCancellationToken()
    {
        _dialogueCts?.Cancel();
        _dialogueCts?.Dispose();
        _dialogueCts = new CancellationTokenSource();
    }

    private void EndDialogue(bool withTopic = false)
    {
        string title = $"{_currentDialoguer.NpcName.Possessive()} account" + (withTopic
        ? $" -\n About {_topic.ToLower()}"
        : " -\n No clear topic");

        if (_manualRecords.Count <= 0 && !_recordChanged) _manualRecords = _previousRecords;

        var finalLog = new LogNote(
            title, 
            _recordText.Segmented(), 
            _manualRecords.Segmented(), 
            _currentDialoguer.Dialogue.DoesItProveAnything()
        );

        LogNote sameLogIfUnique = (LogNote)NotebookManager.Instance.ReturnIfUnique(finalLog, _currentDialoguer.ID);
        if (finalLog == sameLogIfUnique)
            NotebookManager.Instance.AddLogToCharacter(_currentDialoguer.ID, finalLog);
        else sameLogIfUnique.UpdateLog(finalLog);

        _currentDialoguer.SetFace(_currentDialoguer.DefaultEmotion);
        if (_currentDialoguer.FirstTimeSpeaking)
        {
            NotebookManager.Instance.AddCharacter(_currentDialoguer.ID);
            _currentDialoguer.FirstTimeSpeaking = false;
        }
        _currentDialoguer = null;
        _recordText = new();

        _previousRecords = _manualRecords;
        _manualRecords = new();
        _recordChanged = false;
        _recordedContentInSession.Clear();
        m_popActivity.Raise();
        
    }
    
    
    public bool CheckDialogue(SerializableGuid guid) => _dialogueNodesTalked.Contains(guid);
}

