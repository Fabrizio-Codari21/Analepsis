using System;

public interface IActivity
{

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    public void Resume();
    public void Pause();

    public void Stop();

}