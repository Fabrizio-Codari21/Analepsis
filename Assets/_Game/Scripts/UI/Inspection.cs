using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inspection : MonoBehaviour,IActivity
{
    [SerializeField] private InspectionInputReader m_inputReader;
    [SerializeField] private Transform m_inspectRoot;
    [SerializeField] private Camera m_camera;
    [SerializeField] private InspectableEvent m_onInspect;
    [SerializeField] private IActivityEvent m_onActivity;
    [SerializeField] private EventChannel m_popEvent;
    [SerializeField] private BoolEventChannel m_cursorEnable;
    
    [Header("Zoom")]
    [SerializeField] private RawImage m_objectRawImage;

    [SerializeField,Range(0f,1f)] private float m_zoomScaleSensitive;
    [SerializeField] private float m_zoomScaleFactor = 100f;
    [SerializeField] private float m_maxScale;
    [SerializeField] private float m_minScale;
   [SerializeField] private float m_planeRotationSpeed = 0.2f;
   
   private Vector2 _lastDirectionFromCenter;
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
        if (enable) { BeginPlaneRotation(); }
        
    }
    private void Rotate(Vector2 rotation)
    {
        m_inspectRoot.Rotate(Vector3.up, -rotation.x , Space.World);
        m_inspectRoot.Rotate(Vector3.right, rotation.y, Space.World);
    }


    private void PlaneRotation(Vector2 delta)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransform rect = m_objectRawImage.rectTransform;
        Vector2 center = RectTransformUtility.WorldToScreenPoint(null, rect.position);
        Vector2 currentDirectionFromCenter = (mousePos - center).normalized;
        
        float signedAngle = Vector2.SignedAngle(
            _lastDirectionFromCenter,
            currentDirectionFromCenter
        );

        m_inspectRoot.Rotate(
            m_camera.transform.forward,
            signedAngle,
            Space.World
        );
        
        _lastDirectionFromCenter = currentDirectionFromCenter;
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
        //Debug.Log("3");
       
        NotebookManager.instance.SaveClueToNotebook
            ($"{item.clueInfo.clueId} \n - Action {ActionTimer.instance.maxActions - ActionTimer.instance.actionsLeft}",
            item.clueInfo);

        m_camera.orthographicSize = item.size;
        m_onActivity.Raise(this);
    }
    
    private void Exit()=> m_popEvent?.Raise();
    
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
    
    
    private void BeginPlaneRotation()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransform rect = m_objectRawImage.rectTransform;
        Vector2 center = RectTransformUtility.WorldToScreenPoint(null, rect.position);
        _lastDirectionFromCenter = (mousePos - center).normalized;
    }

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;
    public void Resume()
    {
      OnResume?.Invoke();
      m_inputReader.SetEnable();
      m_inputReader.Rotate += Rotate;
      m_inputReader.DragPressed += RotateStart;
      m_inputReader.Scroll += Zoom;
      m_inputReader.Exit  += Exit;
      m_inputReader.PlaneRotate += PlaneRotation;
      gameObject.SetActive(true);
      m_cursorEnable.Raise(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_inputReader.SetEnable(false);
        m_inputReader.Rotate -= Rotate;
        m_inputReader.DragPressed -= RotateStart;
        m_inputReader.Scroll -= Zoom;
        m_inputReader.Exit  -= Exit;
        m_inputReader.PlaneRotate -= PlaneRotation;
        gameObject.SetActive(false);
        m_cursorEnable.Raise(false);
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }

    public bool CanPopWithKey()
    {
       return true;
    }
}


