using UnityEngine;
using System;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CcInputHandler : MonoBehaviour,IActivity
{
   [SerializeField] private CCInputReader m_reader;
   [SerializeField] private IActivityEvent m_readerEvent;
   [SerializeField] private CinemachineInputAxisController m_cameraAxisController;
   
   public event Action<Vector2> Move = delegate { };
   public event Action InteractPressed = delegate { };
   public event Action InteractReleased = delegate { };

   private void Start()
   {
      InitReaderAction();

      foreach (var controller in m_cameraAxisController.Controllers)
      {
         controller.Input.InputAction = InputActionReference.Create(m_reader.InputAction.Player.Look); // agrego camera input action reference
      }
      m_readerEvent.Raise(this); // BASE ACTIVITY EN TEORIA
   }
   

   /// <summary>
   /// Este metodos es para que input reader susbcribirse a actiones de este clase y para invokarlom
   /// si quirene ser mas optimada puede cambiarse que sea metodos con firma y susbribe en enable or disable,
   /// o tambien puede manejar bien la subscrition de otros method a la hora de subscribirse este clase
   /// </summary>
   private void InitReaderAction()
   {
      m_reader.Move += dir => Move?.Invoke(dir);
      m_reader.InteractPressed +=  () => InteractPressed?.Invoke();
      m_reader.InteractReleased += () => InteractReleased?.Invoke();
    
   }


   public void Resume()
   {
      m_reader.SetEnable();
   }

   public void Pause()
   {
      m_reader.SetEnable(false);
   }

   public void Stop()
   {
      Pause();
   }
}