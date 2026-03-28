using System;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour,IActivity
{
    
    [SerializeField] private Button m_button;
    [SerializeField] private EventChannel m_popEvent;
    [SerializeField] private BoolEventChannel m_cursorEnableChannel;
    
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Resume()
    { 
        gameObject.SetActive(true); 
        m_button.onClick.AddListener(m_popEvent.Raise);
        m_cursorEnableChannel?.Raise(true);
        OnResume?.Invoke();
    }

    public void Pause()
    { 
        gameObject.SetActive(false);
        m_button.onClick.RemoveAllListeners();
        m_cursorEnableChannel?.Raise(false);
        OnPause?.Invoke();
    }

    public void Stop()
    {
        Pause();  
        OnStop?.Invoke();
    }
}