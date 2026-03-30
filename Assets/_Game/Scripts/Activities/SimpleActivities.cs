using System;
using UnityEngine;

public class SimpleActivities : MonoBehaviour, IActivity
{
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public bool CanPopWithKey()
    {
        return true;
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
