using System;
using UnityEngine;

public class UIManager : RegulatorSingleton<UIManager>
{
    [SerializeField] private Transform m_optionRoot;

    [SerializeField] private GlobalInputReader m_inputReader;
    
}


public class Menu : MonoBehaviour,IActivity
{
    
    public void Resume()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
