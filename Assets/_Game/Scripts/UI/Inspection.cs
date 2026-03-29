using System;
using UnityEngine;
public class Inspection : MonoBehaviour,IActivity
{
    [SerializeField] private InspectionInputReader m_inputReader;
    [SerializeField] private Transform m_inspectRoot;
    [SerializeField] private Camera m_camera;
    [SerializeField] private InspectableEvent m_onInspect;
    [SerializeField] private IActivityEvent m_onActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;
    private void Start()
    {
         m_onInspect.OnEventRaised += Inspect;
         gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        m_onInspect.OnEventRaised -= Inspect;
    }

    private void RotateStart(bool enable)
    {
        m_camera.enabled = enable;
    }
    private void Rotate(Vector2 rotation)
    {
        m_inspectRoot.Rotate(Vector3.up, -rotation.x , Space.World);
        m_inspectRoot.Rotate(Vector3.right, rotation.y, Space.World);
    }


    public void Inspect(IInspectable inspectable)
    {
        foreach (Transform child in m_inspectRoot)
        {
            child.SetParent(null); 
            Destroy(child.gameObject);
        }


        Instantiate(inspectable.GetInspectItem().gameObject, m_inspectRoot);
     
        m_onActivity.Raise(this);
    }

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    public void Resume()
    {
      OnResume?.Invoke();
      m_inputReader.Rotate += Rotate;
      m_inputReader.DragPressed += RotateStart;
      gameObject.SetActive(true);
      m_cursorEnable.Raise(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_inputReader.Rotate -= Rotate;
        m_inputReader.DragPressed -= RotateStart;
        gameObject.SetActive(false);
        m_cursorEnable.Raise(false);
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }
    
    
}


