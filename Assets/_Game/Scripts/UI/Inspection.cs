using System;
using System.Net.NetworkInformation;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
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
    [SerializeField] private GameObject m_flashbackIndication;
    [SerializeField] private BoolEventChannel enableFlashback;
    [SerializeField] private BoolEventChannel m_updatePOI;
    [SerializeField] private ItemEventChannel itemEvent;
    [SerializeField] private StringEventChannel poiInfo;
    [SerializeField] private TMP_Text m_poiText;
    
    [Header("Zoom")]
    [SerializeField] private RawImage m_objectRawImage;

    [SerializeField,Range(0f,1f)] private float m_zoomScaleSensitive;
    [SerializeField] private float m_zoomScaleFactor = 100f;
   
    [SerializeField] private float m_planeRotationSpeed = 0.2f;
    [SerializeField] private LayerMask m_layerMask;
    private float _maxScale;
    private float _minScale;
    ItemReference _currentItem;
    private float _currentZoom;
    private Vector2 _lastDirectionFromCenter;
    
    private Sequence _poiSequence;
    private void Start()
    {
         m_onInspect.OnEventRaised += Inspect;

         m_poiText.alpha = 0f;
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

        signedAngle *= m_planeRotationSpeed;
        m_inspectRoot.Rotate(m_camera.transform.forward, signedAngle, Space.World);
        
        _lastDirectionFromCenter = currentDirectionFromCenter;
    }
    
    private void Inspect(IInspectable inspectable)
    {
        foreach (Transform child in m_inspectRoot)
        {
            Destroy(child.gameObject);
        }

        _currentItem = inspectable.GetItemReference();
        var inspectItem = _currentItem.GetInspectItem();

        Instantiate(inspectItem.gameObject, m_inspectRoot);

        _maxScale = inspectItem.renderCameraScaleMax;
        _minScale = inspectItem.renderCameraScaleMin;

        
        _currentZoom = (_maxScale + _minScale) / 2;
        m_camera.orthographicSize = _currentZoom;

        
        _hasFlashback = NotebookManager.Instance.HasAllPois(inspectItem);
        m_flashbackIndication.SetActive(_hasFlashback);
        
        _lastDirectionFromCenter = Vector2.zero;

        itemEvent.Raise(inspectItem);
        m_onActivity.Raise(this);
    }

    private void UpdatePoi(bool enable)
    {
        m_flashbackIndication.SetActive(enable);
        _hasFlashback = enable;
    }

    private void ShowPoi(string info)
    {
        if (_poiSequence.isAlive)
        {
            _poiSequence.Stop();
        }
        if (m_poiText == null) return;
        
        
        m_poiText.text = info;
        m_poiText.alpha = 0f; 
        _poiSequence =Sequence.Create()
          
            .Group(Tween.Alpha(m_poiText, endValue: 1f, duration: 0.5f))
        
            .ChainDelay(2f)
            
            .Chain(Tween.Alpha(m_poiText, endValue: 0f, duration: 1f))
            .OnComplete(() => m_poiText.text = string.Empty);
    }
    private void Exit() 
    {
        m_popEvent?.Raise(); 
    }
    private bool _hasFlashback = false;
    private void TryExitByFlashback()
    {
        if(!_hasFlashback) return;
        enableFlashback.Raise(true);
        Exit();
    }
    private void Zoom(Vector2 zoom)
    {
        m_camera.enabled = true;
        float delta = zoom.y * m_zoomScaleSensitive * m_zoomScaleFactor * 0.01f;
        _currentZoom -= delta;

        _currentZoom = Mathf.Clamp(_currentZoom, _minScale, _maxScale);

        m_camera.orthographicSize = _currentZoom;
     
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
        m_camera.enabled = true;
        m_inputReader.SetEnable();
        m_inputReader.Rotate += Rotate;
        m_inputReader.DragPressed += RotateStart;
        m_inputReader.SeeFlashback += TryExitByFlashback;
        m_inputReader.Touch += ExecuteTouch;
        m_updatePOI.OnEventRaised += UpdatePoi;
        m_inputReader.Scroll += Zoom;
        m_inputReader.Exit  += Exit;
        m_inputReader.PlaneRotate += PlaneRotation;
        poiInfo.OnEventRaised += ShowPoi;
        gameObject.SetActive(true);
        m_cursorEnable.Raise(true);
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_camera.enabled = false;
        m_inputReader.SetEnable(false);
        m_inputReader.Rotate -= Rotate;
        m_inputReader.DragPressed -= RotateStart; 
        m_inputReader.SeeFlashback -= TryExitByFlashback;
        m_inputReader.Touch -= ExecuteTouch;
        m_updatePOI.OnEventRaised -= UpdatePoi;
        m_inputReader.Scroll -= Zoom;
        m_inputReader.Exit  -= Exit;
        m_inputReader.PlaneRotate -= PlaneRotation;
        poiInfo.OnEventRaised -= ShowPoi;
        gameObject.SetActive(false);
        m_cursorEnable.Raise(false);
    }
    

    
    private ITouch GetTouchAtScreenPos(Vector2 mousePos)
    {
        RectTransform rectTransform = m_objectRawImage.rectTransform;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners); 

        float u = (mousePos.x - corners[0].x) / (corners[2].x - corners[0].x);
        float v = (mousePos.y - corners[0].y) / (corners[1].y - corners[0].y);

        if (u < 0 || u > 1 || v < 0 || v > 1) return null;

        Ray ray = m_camera.ViewportPointToRay(new Vector3(u, v, 0));

      
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f,m_layerMask))
        {
          
            Debug.DrawLine(ray.origin, hit.point, Color.green, 2f);
            Debug.DrawRay(hit.point, Vector3.up * 0.1f, Color.yellow, 2f);
            Debug.DrawRay(hit.point, Vector3.right * 0.1f, Color.yellow, 2f);
        
            Debug.Log(hit.transform.name);
            return hit.collider.TryGetComponent(out ITouch touch) ? touch : null;
        }
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);
        
        return null;
    }
    private void ExecuteTouch()
    {
        GetTouchAtScreenPos(Input.mousePosition)?.Touch();
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



