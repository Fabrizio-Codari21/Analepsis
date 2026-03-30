using System;
using UnityEngine;
using UnityEngine.UI;

public class Inspection : MonoBehaviour,IActivity
{
    [SerializeField] private InspectionInputReader m_inputReader;
    [SerializeField] private Transform m_inspectRoot;
    [SerializeField] private Camera m_camera;
    [SerializeField] private InspectableEvent m_onInspect;
    [SerializeField] private IActivityEvent m_onActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;
    
    [Header("Zoom")]
    [SerializeField] private RawImage m_objectRawImage;

    [SerializeField,Range(0f,1f)] private float m_zoomScaleSensitive;
    [SerializeField] private float m_zoomScaleFactor = 100f;
    [SerializeField] private float m_maxScale;
    [SerializeField] private float m_minScale;
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
    
    private void Inspect(IInspectable inspectable)
    {
        foreach (Transform child in m_inspectRoot)
        {
            child.SetParent(null); 
            Destroy(child.gameObject);
        }
        var item = inspectable.GetInspectItem();
        Instantiate(item.gameObject,m_inspectRoot);
        Debug.Log("3");
        NotebookManager.instance.SaveClueToNotebook(item.clueInfo.clueId, item.clueInfo);
        m_camera.orthographicSize = item.size;
        m_onActivity.Raise(this);
    }
    


    private void Zoom(Vector2 zoom)
    {
       
        RectTransform rectTrans = m_objectRawImage.rectTransform;

      
        float delta = zoom.y * m_zoomScaleSensitive * m_zoomScaleFactor;
        
        float newWidth = rectTrans.sizeDelta.x + delta;
        float newHeight = rectTrans.sizeDelta.y + delta;
        
        newWidth = Mathf.Clamp(newWidth, m_minScale, m_maxScale);
        newHeight = Mathf.Clamp(newHeight, m_minScale, m_maxScale);
        
        rectTrans.sizeDelta = new Vector2(newWidth, newHeight);
    }
    

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    public void Resume()
    {
      OnResume?.Invoke();
      m_inputReader.Rotate += Rotate;
      m_inputReader.DragPressed += RotateStart;
      m_inputReader.Scroll += Zoom;
      gameObject.SetActive(true);
      m_cursorEnable.Raise(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_inputReader.Rotate -= Rotate;
        m_inputReader.DragPressed -= RotateStart;
        m_inputReader.Scroll -= Zoom;
        gameObject.SetActive(false);
        m_cursorEnable.Raise(false);
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }
    
    
}


