using System;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour,IActivity
{
    
    [SerializeField] private Button m_button;
    [SerializeField] private EventChannel m_popEvent;
    
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
       OnResume?.Invoke();
    }

    public void Pause()
    {
        gameObject.SetActive(false);
        m_button.onClick.RemoveAllListeners();
        OnPause?.Invoke();
    }

    public void Stop()
    {
        Pause();  
        OnStop?.Invoke();
    }
}