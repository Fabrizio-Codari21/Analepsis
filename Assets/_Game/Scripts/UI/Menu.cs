using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour,IActivity
{
    
    [SerializeField] private Button m_button;
    [SerializeField] private EventChannel m_popEvent;
    [SerializeField] private BoolEventChannel m_cursorEnableChannel;

    
    [SerializeField] private Button m_reloadButton;
    public bool CanPopWithKey() => m_canPop;

    [SerializeField] private bool m_canPop;

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
        
        m_reloadButton.onClick.AddListener(Reload);
        m_cursorEnableChannel?.Raise(true);
        OnResume?.Invoke();
    }

    public void Pause()
    { 
        gameObject.SetActive(false);
        m_button.onClick.RemoveAllListeners();
        m_reloadButton.onClick.RemoveAllListeners();
        m_cursorEnableChannel?.Raise(false);
        OnPause?.Invoke();
    }

    public void Stop()
    {
        Pause();  
        OnStop?.Invoke();
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void Quit()
    {
        Application.Quit();
    }


    public void Back()
    {
        m_popEvent.Raise();
    }
    
    
}