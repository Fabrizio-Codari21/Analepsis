using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Linq;

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
        m_dialogueEvent.OnEventRaised += dialogable => _ = Speak(dialogable);
    }
    private async UniTaskVoid Speak(IDialogable dialogable)   
    {
        m_pushActivity.Raise(this);
        _currentDialoguer = dialogable;
        _currentDialoguer.Dialogue._hiddenProof.Clear();
        m_dialogueView.ClearDialogues();
        m_dialogueView.SetSpeakerName(dialogable.NpcName);
        await UnfoldDialogue(true);
        await PlayDialogueNode(dialogable.Dialogue.startingNode);
    }
    private async UniTask UnfoldDialogue(bool isOpening)
    {
        print("dialogue");
        
        if (isOpening)
        {
            m_dialogueView.gameObject.transform.localScale -= new Vector3(0, m_dialogueView.gameObject.transform.localScale.y, 0);

            while (m_dialogueView.gameObject.transform.localScale.y < 1)
            {
                m_dialogueView.gameObject.transform.localScale += new Vector3(0, 0.02f, 0);
                await UniTask.Delay(20);
            }
        }
        else
        {
            while (m_dialogueView.gameObject.transform.localScale.y > 1)
            {
                m_dialogueView.gameObject.transform.localScale -= new Vector3(0, 0.02f, 0);
                await UniTask.Delay(20);
            }

            m_dialogueView.gameObject.transform.localScale -= new Vector3(0, m_dialogueView.gameObject.transform.localScale.y, 0);
        }

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
        if (response.nextNode == null)
        {
            await UnfoldDialogue(false);
            EndDialogue();
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
            await UnfoldDialogue(false);
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
        m_recordNoteEvent.Raise(new LogNote(
            $"{_currentDialoguer.NpcName}{(_currentDialoguer.NpcName.Last() == 's' ? "'" : "'s")} account -\n Action {ActionTimer.Instance.CurrentAction()}"
            ,_recordText
            ,_currentDialoguer.Dialogue.DoesItProveAnything()));

        _currentDialoguer = null;
        _recordText = String.Empty;
        m_popActivity.Raise();
        
    }
    
    
    public bool CheckDialogue(SerializableGuid guid) => _dialogueNodesTalked.Contains(guid);
}