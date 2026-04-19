using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class FlashbackManager : MonoBehaviour
{
 
    [SerializeField] private Material highlightShader;
    
    [Header("Transition")]
    [SerializeField] private FlashbackContext m_ctx;
    public BoolEventChannel enableFlashback;
    
    
    [SerializeField] private DynamicTextSetting displaySetting;
    [SerializeField] private ItemEventChannel itemEvent;
    [SerializeField] private RecordNoteEvent m_recordNote;
    private Interactable _flashbackObject;
    private Item _currentItem;
    DynamicText flashbackClueDisplay;
     
    private AsyncFiniteStateMachine<FlashbackState> _stateMachine;
    
    private void Start()
    {
        FsmSetup();
       
    }
    
    private void SetCurrentItem(Item item) => _currentItem = item;

    private void OnEnable()
    {
        enableFlashback.OnEventRaised += OnFlashback;
        itemEvent.OnEventRaised += SetCurrentItem;
    }

    private void OnDisable()
    {
        enableFlashback.OnEventRaised -= OnFlashback;
        itemEvent.OnEventRaised -= SetCurrentItem;
    }

    private async void OnFlashback(bool enable)
    {
        try
        {
            if (enable) await _stateMachine.TransitionTo(FlashbackState.Active);
            else await _stateMachine.TransitionTo(FlashbackState.Inactive);
        }
        catch (Exception e)
        {
           Debug.LogError(e);
        }
    }

    private void SpawnFlashObject()
    {
        if(!_currentItem) return;
        if (_flashbackObject) return;
        
        var fb = _currentItem.flashbackInfo;
        NotebookManager.Instance.UpdateFlashbackInfo(_currentItem,fb.info);
        var t = TransformKeyManager.Instance.GetTransform(fb.key);

        if (!t)
        {
            Debug.LogError("No transform key found or not transform instance create, check plis");
            return;
        }
        var position = t.position + fb.offset;
        var rotation = t.rotation;

        _flashbackObject = Instantiate(fb.characterPrefab,position,rotation);

        if (_flashbackObject.TryGetComponent<TextComponent>(out var textComponent))
        {
            textComponent.Init(fb.info);
        }
    }


    private void Despawn()
    {
        Debug.Log("Despawn");
        
        
        Destroy(_flashbackObject.gameObject);
        _flashbackObject = null;
        _currentItem = null;
    }
    private void FsmSetup()
    {
        var inactive = new FlashbackInactiveState(m_ctx);
        var active = new FlashbackActiveState(
            m_ctx,
            SpawnFlashObject,
            Despawn
        );
        _stateMachine = new AsyncFiniteStateMachine<FlashbackState>
                .Builder()
            .State(FlashbackState.Inactive, inactive)
            .State(FlashbackState.Active, active)
            .Build(FlashbackState.Inactive);
    }
 
}
[Serializable]
public class FlashbackContext
{
    [Header("Transition")]
    public UITransitionEffect transitionEffect;
    public FlashbackInputReader flashbackInputReader;
    
    [Header("Material")]
    public Material flashbackMaterial;
    [Range(0.5f,1f)]public float targetValue;
    public float lerpDuration;
    
    public BoolEventChannel enableFlashback;
 

}


public enum FlashbackState
{
    Active,
    Inactive
}

public class FlashbackActiveState : IAsyncState  // Enter flashback 
{
    private readonly FlashbackContext _context;
    private readonly Action _spawn;
    private readonly Action _despawn;
    public FlashbackActiveState(FlashbackContext context,Action Spawn,Action Despawn)
    {
       _context = context;
       _spawn = Spawn;
       _despawn = Despawn;
    }

    public async Task OnEnter()
    {
        var fadeTask = _context.transitionEffect.FadeIn();
        var matTask =LerpMaterialFloat(
            _context.flashbackMaterial,
            "_Control",
            0f,
            _context.targetValue,
            _context.lerpDuration
        );
        await UniTask.WhenAll(fadeTask, matTask);
        _spawn?.Invoke();

        _context.flashbackInputReader.SetEnable();
        _context.flashbackInputReader.ExitFlashback += Exit;
        await _context.transitionEffect.FadeOut();
     
    }

    private void Exit()
    {
        
        _context.enableFlashback.Raise(false);
    }

    public void Update()
    {
      
    }

    public async Task OnExit()
    {
       
        await _context.transitionEffect.FadeIn();
        
       
        _context.flashbackInputReader.ExitFlashback -= Exit;
        _context.flashbackInputReader.SetEnable(false);
  
        _despawn?.Invoke();
    
        var fadeTask = _context.transitionEffect.FadeOut();
        var matTask =LerpMaterialFloat(
            _context.flashbackMaterial,
            "_Control",
            _context.targetValue,
            0f,
            _context.lerpDuration
        );
        
        await UniTask.WhenAll(matTask,fadeTask);
     
    }
    
    
   private  async UniTask LerpMaterialFloat(
        Material mat,
        string property,
        float from,
        float to,
        float duration,
        CancellationToken token = default)
    {
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float value = Mathf.Lerp(from, to, t);

            mat.SetFloat(property, value);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        mat.SetFloat(property, to);
    }
}

public class FlashbackInactiveState : IAsyncState // Back
{
    private readonly FlashbackContext _context;
    public FlashbackInactiveState(FlashbackContext context)
    {
        _context = context;
    }
    public Task OnEnter()
    {
        return Task.CompletedTask;
    }
    

    public void Update()
    {
      
    }

    public Task OnExit()
    {
        return Task.CompletedTask;
    }
}

