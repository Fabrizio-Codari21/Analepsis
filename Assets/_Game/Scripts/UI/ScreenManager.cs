using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScreenManager : RegulatorSingleton<ScreenManager>
{
    [ShowInInspector, ReadOnly] private StackManager<IActivity> _activities = new StackManager<IActivity>();
    
    [SerializeField] private IActivityEvent m_pushEvent;
    [SerializeField] private EventChannel m_popInputEvent;
    [SerializeField] private GlobalInputReader  m_globalReader;
    
    [SerializeField] private Menu m_menu;
    protected override void Awake()
    {
        base.Awake();
        m_pushEvent.OnEventRaised += Push;
        m_popInputEvent.OnEventRaised += Pop;
        m_globalReader.EscapePressed += Escape;
    }

    private void Start()
    {
        m_menu = Instantiate(m_menu, transform);
    }

    private void Push(IActivity activity)
    {
        _activities.Push(activity);
    }

    private void Pop()
    {
        _activities.PopSaveRoot();
    }


    private void Escape()
    {
        if(_activities.IsOnlyRoot()|| !_activities.Current.CanPopWithKey()) m_pushEvent.Raise(m_menu);
        else m_popInputEvent.Raise();
    }


    private void OnDestroy()
    {
        if(!HasInstance || instance != this) return;
        
        m_pushEvent.OnEventRaised -= Push;
        m_popInputEvent.OnEventRaised -= Pop;
        m_globalReader.EscapePressed -= Escape;
    }
}