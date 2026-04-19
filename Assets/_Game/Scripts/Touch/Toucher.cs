using System;
using UnityEngine;

public class Toucher : MonoBehaviour
{
    private Camera _cam ;
    
    [SerializeField] private MenuInputReader _inputReader;
    [SerializeField] private float m_scanTime;
    [SerializeField] private LayerMask m_layer;
    [SerializeField] private float m_range;
    private ITouch _last;
    private float _lastScanTime;

 
    private void Awake()
    {
        _cam = GetComponentInParent<Camera>();
        if(_cam == null) _cam = Camera.main;
    }

    private void Start()
    {
        _inputReader.Touch += Touch;
    }
    

    private void Update()
    {
        _lastScanTime += Time.deltaTime;
        
        if(_lastScanTime < m_scanTime) return;
        
        _lastScanTime = 0;
        
        Handle();
    }

    private void Touch()
    {
        _last?.Touch();
    }

    private void Handle()
    {
        if (InputsManager.Instance.IsPointerOverUI())
        {
            ResetTouch();
            return;
        }

        if (!_cam) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_layer);
        
        

        
        if (!hasHit || !hit.collider.TryGetComponent(out ITouch currentTouch))
        {
            ResetTouch();
            return;
        }

        float distanceToHit = Vector3.Distance(transform.position, hit.collider.ClosestPoint(transform.position));
        if (distanceToHit > m_range)
        {
            ResetTouch();
            return;
        }

        if (currentTouch == _last) return;

        _last = currentTouch;
        _last.Focus();
    }
    private void ResetTouch()
    {
        if(_last == null) return;
        _last.Unfocus();
        _last = null;
    }

    private void OnDestroy()
    {
        _inputReader.Touch -= Touch;
    }
}

public interface ITouch : IFocus
{
    void Touch();
}
