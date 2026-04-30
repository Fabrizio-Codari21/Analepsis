using UnityEngine;
using System;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CcInputHandler : MonoBehaviour
{
   [SerializeField] private CCInputReader m_reader;
   [SerializeField] private IActivityEvent m_readerEvent;
   [SerializeField] private CinemachineInputAxisController m_cameraAxisController;
   [SerializeField] private CinemachineCamera  m_camera;
   [SerializeField] private Transform m_cameraTransform;
   [SerializeField] private EventChannel m_openBook;
   [SerializeField] private EventChannel m_openTheoryBoard;
   [SerializeField] private BoolEventChannel m_flashback;
   public event Action<Vector2> Move = delegate { };
   public event Action InteractPressed = delegate { };
   public event Action InteractReleased = delegate { };
   private IActivity activity;
   
   [SerializeField] private TransformEventChannel m_cameraTransformEvent;


   private bool _onFlashBack = false;
    private void Start()
   {
        foreach (var controller in m_cameraAxisController.Controllers)
        {
            controller.Input.InputAction = InputActionReference.Create(m_reader.InputAction.Player.Look); // agrego camera input action reference
        }
        activity = GetComponent<IActivity>();
        activity.OnResume += Resume;
        activity.OnPause += Pause;
        activity.OnStop += Stop;
      
        m_flashback.OnEventRaised += Flashback;
        m_reader.Move += Movement;
        m_reader.InteractPressed += InteractStart;
        m_reader.InteractReleased += InteractStop;
        m_reader.OpenNotebook += TryOpenNotebook;
        m_reader.OpenTheoryBoard += TryOpenTheoryBoard;
        m_cameraTransformEvent.OnEventRaised += SetCamera;
        m_readerEvent.Raise(activity); // BASE ACTIVITY EN TEORIA
      
   }


    private void Flashback(bool enable) => _onFlashBack = enable;


   private void Movement(Vector2 dir)
   {
      Move?.Invoke(dir);
   }


   private void InteractStart()
   {
      InteractPressed?.Invoke();
   }

   private void InteractStop()
   {
      InteractReleased?.Invoke();
   }

   private void TryOpenNotebook()
   {
      if(_onFlashBack) return;
      m_openBook?.Raise();
   }


   private void TryOpenTheoryBoard()
   {
      if(_onFlashBack) return;
      m_openTheoryBoard?.Raise();
   }

   private void Resume()
   {
      m_reader.SetEnable();
   }

   private void Pause()
   {
      m_reader.SetEnable(false);
   }

   private void Stop()
   {
      Pause();
   }


   private void SetCamera(Transform target)
   {
      m_camera.ForceCameraPosition(target.position, target.rotation);

      m_camera.Follow = target;     
   }

   private void OnDestroy()
   {
      m_cameraTransformEvent.OnEventRaised -= SetCamera;

      m_flashback.OnEventRaised -= Flashback;
      
      m_reader.Move -= Movement;
      m_reader.InteractPressed -= InteractStart;
      m_reader.InteractReleased -= InteractStop;
      m_reader.OpenNotebook -= TryOpenNotebook;
      m_reader.OpenTheoryBoard -= TryOpenTheoryBoard;
   }
}