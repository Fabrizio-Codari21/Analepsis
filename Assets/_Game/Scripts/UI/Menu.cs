
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour,IActivity
{
    
    [SerializeField] private Button _button;
    [SerializeField] private EventChannel m_inputPop;
    [SerializeField] private IActivityEvent m_inputActivity;
    [SerializeField] private IActivityEvent m_uiActivity;
    

    public void Resume()
    {
       gameObject.SetActive(true);
    }

    public void Pause()
    {
        gameObject.SetActive(false);
    }

    public void Stop()
    {
        gameObject.SetActive(false);
    }
}