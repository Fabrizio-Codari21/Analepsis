using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIManager : RegulatorSingleton<UIManager>,IActivity
{
    [SerializeField] private Transform m_optionRoot;

    [SerializeField] private GlobalInputReader m_inputReader;

    [SerializeField] private IActivity m_menu;
    
    [ShowInInspector,ReadOnly] private StackManager<IActivity>  _activities = new StackManager<IActivity>();

    [SerializeField] private EventChannel m_popEventChannel;

    private void Start()
    {
        _activities.Push(this);
    }

    public void Resume()
    {
        m_inputReader.EscapePressed += Menu;
    }

    public void Pause()
    {
       m_inputReader.EscapePressed -= Menu;
    }

    public void Stop()
    {
      Pause();
    }

    private void Menu()
    {
        _activities.Push(m_menu);
    }
}