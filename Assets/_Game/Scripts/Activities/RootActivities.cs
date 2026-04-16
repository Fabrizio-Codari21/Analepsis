using System;
using UnityEngine;

/// <summary>
/// este se usa en caso de si no hay un  "Player" 
/// </summary>
public class RootActivities : MonoBehaviour, IActivity
{
    [SerializeField] private IActivityEvent m_pushEvent;
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    private void Start()
    {
        m_pushEvent.Raise(this);
    }

    public bool CanPopWithKey()
    {
        return false;
    }

    void IActivity.Pause()
    {
        OnPause?.Invoke();
    }

    void IActivity.Resume()
    {
        OnResume?.Invoke();
    }

    void IActivity.Stop()
    {
        OnStop?.Invoke();
    }
}