using System;
using UnityEngine;

public class UIManager : RegulatorSingleton<UIManager>
{
    [SerializeField] private Transform m_optionRoot;

    [SerializeField] private GlobalInputReader m_inputReader;

    [SerializeField] private IActivity m_menu;
    
   
    [SerializeField] private EventChannel m_popEventChannel;

    private void Start()
    {
        
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
       
    }
}
