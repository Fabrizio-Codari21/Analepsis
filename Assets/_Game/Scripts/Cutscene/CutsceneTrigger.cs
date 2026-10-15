using Cysharp.Threading.Tasks;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour, IActivity
{

    public CutsceneInfo cutsceneInfo;
    [Space(10)] public CutsceneManager manager;

    public UniTask StartDialogue() => _ = manager.PlayDialogue(cutsceneInfo, !cutsceneInfo.startWithTheRightCharacter);

    #region INTERACTABLE

    [Space(10), Header("EVENTS")]
    [SerializeField] private IActivityEvent m_pushActivity;
    [SerializeField] private EventChannel m_popActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    [Space(10), Header("INTERACTION")]
    [SerializeField] private DynamicTextSetting m_nameTextSetting;
    [SerializeField] private Vector3 m_textPositionOffset;
    private IInteractable _interact;
    private ITipProvider _tipProvider;
    private DynamicText _text;
    

    void Start()
    {
        _interact = TryGetComponent<IInteractable>(out var t) 
            ? GetComponent<IInteractable>() 
            : gameObject.AddComponent<Interactable>();

        _interact.OnFocus += SpawnName;
        _interact.OnUnfocus += DespawnName;
        _interact.OnStart += DespawnName;
        _interact.OnStart += () => m_pushActivity.Raise(this);

        _tipProvider = GetComponent<ITipProvider>();
        _tipProvider.AddTip(new Tip($"Interact?", TipOrder.InteractionType));
    }

    private void SpawnName()
    {
        _text = FlyweightFactory.Instance.Spawn<DynamicText>(m_nameTextSetting, m_textPositionOffset + transform.position, Quaternion.identity, transform);
        _text.SetText("[CUTSCENE TEST]", 2, m_nameTextSetting.color);
        _ = _text.PlayTypeWriterEffect();
    }

    private void DespawnName()
    {
        if (_text) FlyweightFactory.Instance.Return(_text);
        _text = null;
    }

    public void Resume()
    {
        OnResume?.Invoke();
        m_cursorEnable.Raise(true);
        manager.SetCutscene(this);
        StartDialogue();
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_cursorEnable.Raise(false);
        m_popActivity.Raise();
        //Close();
    }

    public void Stop()
    {
        OnStop?.Invoke();
        OnPause?.Invoke();
        m_cursorEnable.Raise(false);
        m_popActivity.Raise();
        //Close();
    }

    public bool CanPopWithKey()
    {
        return true;
    }

    #endregion
}
