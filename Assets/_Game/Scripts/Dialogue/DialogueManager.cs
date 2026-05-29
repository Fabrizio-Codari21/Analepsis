using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using System.Linq;


public class DialogueManager : Singleton<DialogueManager>,IActivity
{
    [SerializeField] private DialoguerEvent m_dialogueEvent;
    [SerializeField] private DialogueInputReader m_inputReader;
    [SerializeField] private DialogueView m_dialogueView;
    [SerializeField] private Transform m_player;

    [SerializeField] private IActivityEvent m_pushActivity;
    [SerializeField] private EventChannel m_popActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;
    [SerializeField] private float timeToOutDialogue; // se usa cuando la última palabra se la da el npc
    [SerializeField] private NoteEvent noteEvent;
    private CancellationTokenSource  _dialogueCts;
    private IDialogable _currentDialoguer;
    private HashSet<string> _recordedContentInSession = new HashSet<string>();

    private List<string> _manualRecords = new();
    private List<string> _previousRecords = new();
    private string _topic = string.Empty;
    
    
    private DialogueNode _currentNpcNode = null;      
    private DialogueResponse _currentResponseNode = null;
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
        
        m_dialogueView.IsAlreadyRecorded += (content) => _previousRecords.Contains("- " + content);
        
        
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
        m_pushActivity.Raise(this);

        _currentDialoguer = dialogable;
        _currentDialoguer.Dialogue.hiddenProof.Clear();
        m_dialogueView.ClearDialogues();
        m_dialogueView.SetSpeakerName(dialogable.NpcName);

      

        AudioManager.Instance.SelectSFX(SFXType.Player, "FlipForwards");
        _ = AudioManager.Instance.ChangeMusicState(MusicState.Dialogue);

        await m_dialogueView.UnfoldDialogue(
            true, 
            _currentDialoguer.ID.makesEyeContact,
            _currentDialoguer.LookAt, 
            _currentDialoguer.Player);
        
        if (dialogable.Dialogue && dialogable.Dialogue.startingNode != null)
        {
            dialogable.Dialogue.startingNode.PreviousResponse = null;
        }

        await PlayDialogueNode(dialogable.Dialogue.startingNode);

    }

   
    private async UniTask PlayDialogueNode(DialogueNode node) 
    {
        if(node == null) return;
        _dialogueNodesTalked.Add(node.guid);
        
        _currentNpcNode = node;
        if (_currentDialoguer != null)
        {
            NotebookManager.Instance.RecordDialogueProgress(
                _currentDialoguer.ID,           
                _currentDialoguer.Dialogue,    
                node,                           
                _currentResponseNode          
            );
        }
        ResetCancellationToken();
        var token = _dialogueCts.Token;
    
        m_dialogueView.ClearResponses();
        if (node.doesItProveAnything != 0)
        {
            _currentDialoguer?.Dialogue.DiscoverProof(node.doesItProveAnything);
        }

       
        try 
        {
            if(node.characterEmotion != Emotion.None)
            {
                _currentDialoguer?.SetFace(node.characterEmotion);
            }

            // Hay dos maneras de setear las reacciones:
            
            // - La A hace que cada vez que se setee una reacción, el npc va a permanecer en ella
            // hasta que un nodo aclare que tiene que cambiar.
            
            
            // - La B hace que si no se setea una reacción en el nodo siguiente, vuelve por
            // default al idle, asi que hay que marcar varios nodos si queremos que la anim siga.
            // Por ahora dejo la B que me cierra mas, pero despues vemos cual es mas comoda.

            // /*A)*/if(node.characterReaction != Reaction.None)
            //          _currentDialoguer.SetAnimation(node.characterReaction);

            /*B)*/
            if (_currentDialoguer != null)
            {
                _currentDialoguer.SetAnimation(node.characterReaction != Reaction.None ? node.characterReaction : Reaction.Idle);

                await m_dialogueView.PlayDialogueText(node.dialogueText, token,
                    _currentDialoguer.Dialogue.dialogueColor);
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
            string tagToDisplay = string.Empty;
            
            bool wasUnlocked = NotebookManager.Instance.FoundCharacters.Contains(_currentDialoguer.ID) && response.IsNewResponse();
            
            ResponseDialogueButton button = (ResponseDialogueButton)m_dialogueView.CreateResponseButton(response.responseText, tagToDisplay);

            button.AddListener(() => 
            { 
                button.SetInteractable(false);
                _currentResponseNode = response;
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
        
        if (_currentDialoguer != null)
        {
            NotebookManager.Instance.RecordDialogueProgress(
                _currentDialoguer.ID,
                _currentDialoguer.Dialogue,
                response,        
                _currentNpcNode  
            );
        }
        if (response.nextNode == null && _currentDialoguer != null)
        {
            await m_dialogueView.UnfoldDialogue(
                false, 
                _currentDialoguer.ID.makesEyeContact,
                _currentDialoguer.LookAt, 
                _currentDialoguer.Player);

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
            PlayDialogueNode(response.nextNode).Forget();
        }
        else
        {
            await m_dialogueView.UnfoldDialogue(false, _currentDialoguer.ID.makesEyeContact, _currentDialoguer.LookAt, _currentDialoguer.Player);

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
        AudioManager.Instance.SelectSFX(SFXType.Player, "FlipBackwards");
        _ = AudioManager.Instance.ChangeMusicState(MusicState.Default);
        

        _currentDialoguer.SetFace(_currentDialoguer.DefaultEmotion);
        _currentDialoguer.ResetAnimation();
        if (_currentDialoguer.FirstTimeSpeaking)
        {
            NotebookManager.Instance.AddCharacter(_currentDialoguer.ID);           
            _currentDialoguer.FirstTimeSpeaking = false;
        }
        
        
        _currentDialoguer = null;
        _currentNpcNode = null;
        _currentResponseNode = null;
        
        _previousRecords = _manualRecords;
        _manualRecords = new();
        _recordedContentInSession.Clear();
        m_popActivity.Raise();
    }
    public bool CheckDialogue(SerializableGuid guid) => _dialogueNodesTalked.Contains(guid);
}

