using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;



public class DialogueManager : Singleton<DialogueManager>,IActivity
{
    
    [Header("Event")]
    [SerializeField] private DialoguerEvent m_dialogueEvent;
    [SerializeField] private NoteEvent recordNoteEvent;
    
    [SerializeField] private IActivityEvent m_pushActivity;
    [SerializeField] private EventChannel m_popActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;
    
    [Header("Input")]
    [SerializeField] private DialogueInputReader m_inputReader;
    
    [Header("Core")]
    [SerializeField] private DialogueView m_dialogueView;
    [SerializeField] private float timeToOutDialogue; // se usa cuando la última palabra se la da el npc
    
    
    private CancellationTokenSource  _dialogueCts;
    private IDialogable _currentDialoguer;
    private DialogueNode _currentNpcNode = null;      
    private DialogueResponse _currentResponseNode = null;
    [Header("Data")]
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
       
        if (m_inputReader) m_inputReader.Skip -= Skip;
        m_cursorEnable.Raise(false);
    }

    public void Stop()
    { 
        m_dialogueView?.gameObject.SetActive(false);
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
        
        m_dialogueEvent.OnEventRaised += SpeakTo;
    }


    private void OnDestroy()
    {
        m_dialogueEvent.OnEventRaised -= SpeakTo;
    }

    private void SpeakTo(IDialogable dialogable)
    {
         
        
         Speak(dialogable).Forget();
    }
    private async UniTaskVoid Speak(IDialogable dialogable)   
    {
       
        if (dialogable == null)
        {
            return;
        }
        m_pushActivity.Raise(this);
        _currentDialoguer = dialogable;
        if (_currentDialoguer.Dialogue != null)
        {
            _currentDialoguer.Dialogue.hiddenProof?.Clear();
        }
        m_dialogueView.ClearDialogues();
        m_dialogueView.SetSpeakerName(dialogable.DialoguerName);

     

        AudioManager.Instance.SelectSfx(SFXType.Player, "FlipForwards");
        _ = AudioManager.Instance.ChangeMusicState(MusicState.Dialogue);
        
        await m_dialogueView.UnfoldDialogue(true); 
           
        
        if (dialogable.Dialogue && dialogable.Dialogue.startingNode != null)
        {
            dialogable.Dialogue.startingNode.PreviousResponse = null;
        }

        if (dialogable.Dialogue != null && dialogable.Dialogue.startingNode != null)
        {
            await PlayDialogueNode(dialogable.Dialogue.startingNode.SelectAltDialogue());
        }
        else
        {
            await PlayDialogueNode(null);
        }

    }

   
    private async UniTask PlayDialogueNode(DialogueNode node) 
    {
        if(node == null) return;
        _dialogueNodesTalked.Add(node.guid);
        
        _currentNpcNode = node;
        
        // if (_currentDialoguer != null)
        // {
        //     NotebookManager.Instance.RecordDialogueProgress(_currentDialoguer.ID, _currentDialoguer.Dialogue, node, _currentResponseNode);
        // }
        ResetCancellationToken();
        var token = _dialogueCts.Token;
    
        m_dialogueView.ClearResponses();
        if (node.doesItProveAnything != Whodunnit.NoProof)
        {
            foreach (Whodunnit proof in Enum.GetValues(typeof(Whodunnit)))
            {
                if (proof == Whodunnit.NoProof) continue;

           
                if (node.doesItProveAnything.HasFlag(proof))
                {
                    _currentDialoguer?.Dialogue.DiscoverProof(proof);
                }
            }
        }

       
        try 
        {
            if(node.characterEmotion != Emotion.None)
            {
                _currentDialoguer?.SetEmotion(node.characterEmotion);
            }

            // Hay dos maneras de setear las reacciones:
            
            // - La B hace que si no se setea una reacción en el nodo siguiente, vuelve por
            // default al idle, asi que hay que marcar varios nodos si queremos que la anim siga.
            // Por ahora dejo la B que me cierra mas, pero despues vemos cual es mas comoda.
            
            /*B)*/
            if (_currentDialoguer != null)
            {
                _currentDialoguer.SetAnimation(node.characterReaction != Reaction.None ? node.characterReaction : Reaction.Idle);

                await m_dialogueView.PlayNpcDialogue(node, token, _currentDialoguer.Dialogue.dialogueColor);
                // mas que nada para que no siga "hablando" cuando el diálogo ya termino de reproducirse.
                _currentDialoguer.SetAnimation(Reaction.Idle);
            }
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
            
            bool wasUnlocked = NotebookManager.Instance.FoundCharacterGuids.Contains(_currentDialoguer.Guid()) && response.IsNewResponse();
            
            TagButton button = (TagButton)m_dialogueView.CreateResponseButton(response.responseText);

            button.AddListener(() => 
            { 
                button.SetInteractable(false);
                _currentResponseNode = response;
                PlayResponseProcess(response).Forget();  
            });
            button.MarkTag(wasUnlocked);
        }
        await UniTask.NextFrame();
    }

    private async UniTaskVoid PlayResponseProcess(DialogueResponse response)
    {
        ResetCancellationToken();
        var token = _dialogueCts.Token;

        m_dialogueView.ClearResponses();
        
        if (_currentDialoguer != null)
            
        {
            NotebookManager.Instance.RecordDialogueProgress(_currentDialoguer.Guid(), _currentDialoguer.Dialogue, response, _currentNpcNode);
        }
        if (response.nextNode == null && _currentDialoguer != null)
        {
            await m_dialogueView.UnfoldDialogue(false);
            EndDialogue();
            return;
        }
        

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
            PlayDialogueNode(response.nextNode.SelectAltDialogue()).Forget();
        }
        else
        {
            await m_dialogueView.UnfoldDialogue(false);

            EndDialogue();
        }
    }
    
 
    private void ResetCancellationToken()
    {
        _dialogueCts?.Cancel();
        _dialogueCts?.Dispose();
        _dialogueCts = new CancellationTokenSource();
    }

    private void EndDialogue()
    {
        AudioManager.Instance.SelectSfx(SFXType.Player, "FlipBackwards");
        _ = AudioManager.Instance.ChangeMusicState(MusicState.Default);
        
        _currentDialoguer.EndDialogue();
        _currentDialoguer = null;
        _currentNpcNode = null;
        _currentResponseNode = null;
        
        m_popActivity.Raise();
    }
    public bool CheckDialogue(SerializableGuid guid) => _dialogueNodesTalked.Contains(guid);
}

