using System;
using TMPro;
using UnityEngine;

public class Interacter : MonoBehaviour
{
    [SerializeField] private float m_range;
    [SerializeField] private LayerMask m_interactableLayer;
    [SerializeField] private float m_hoverDelay = 0.05f;



    [SerializeField] private Transform m_canvaRoot;
    [SerializeField] private GameObject m_interactCanva;
    private Camera _camera;
    private CcInputHandler _inputHandler;
    
    
    private IInteractable _lastInteractable;
    private float  _hoverTimer;

    public TextMeshProUGUI interactText;

    GameObject canva;


    IActivity activity;
    
    private void OnEnable()
    {
        _inputHandler.InteractPressed += StartInteract;
        _inputHandler.InteractReleased += EndInteract;
    }
    private void OnDisable()
    {
        _inputHandler.InteractPressed -= StartInteract;
        _inputHandler.InteractReleased -= EndInteract; 
    }


    private void Awake()
    {
       _camera = Camera.main;
       _inputHandler = GetComponent<CcInputHandler>();
        activity = GetComponent<IActivity>();
        activity.OnResume += Resume;
        activity.OnPause += Pause;
        activity.OnStop += Stop;
       
    }

    private void Start()
    {
        canva = Instantiate(m_interactCanva, m_canvaRoot);
        interactText.gameObject.SetActive(false);
    }
    private void Update()
    {
        _hoverTimer += Time.deltaTime;
        if (_hoverTimer < m_hoverDelay) return;

        _hoverTimer = 0f;

        HandleInteract();
    }


    private void HandleInteract()
    {
        
        if (!_camera) return;
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit,  Mathf.Infinity,m_interactableLayer) || !hit.collider.TryGetComponent(out IInteractable interactable))
        {
            ResetInteract();
            return;
        }
        float distanceToHit = Vector3.Distance(transform.position, hit.collider.ClosestPoint(transform.position));

        if (distanceToHit > m_range)
        {
            ResetInteract();
            return;
        }
        if (interactable == _lastInteractable) return;
        
        
       
        ResetInteract();
        _lastInteractable = interactable;
        interactable.Focus();
        
    }
    
     
    private void StartInteract()
    {
        _lastInteractable?.InteractStart();
    }

    private void EndInteract()
    {
        _lastInteractable?.InteractEnd();
    }
    
    private void ResetInteract()
    {
        if (_lastInteractable == null) return;
        _lastInteractable.Unfocus();
        _lastInteractable = null;

    }


    private void Resume()
    {
        canva.gameObject.SetActive(true);
    }

    private void Pause()
    {
        canva.gameObject.SetActive(false);
    }

    private void Stop()
    {
        Pause();
    }

}
