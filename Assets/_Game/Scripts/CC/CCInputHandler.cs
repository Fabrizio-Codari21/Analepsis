using UnityEngine;
using System;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CCInputHandler : MonoBehaviour
{
   [SerializeField] private CCInputReader m_reader;

   [SerializeField] private InputReaderEvent m_readerEvent;
   
   [SerializeField] private CinemachineInputAxisController m_inputAxisController;
   
   public event Action<Vector2> Move = delegate { };
   public event Action<Vector2> Look = delegate { };
   public event Action InteractPressed = delegate { };
   public event Action InteractReleased = delegate { };

   private void Start()
   {
      InitReaderAction();
      
      var controllers = m_inputAxisController.Controllers;

      var lookAction = m_reader.InputActions.Player.Look;

      foreach (var controller in controllers)
      {
         controller.Input.InputAction = InputActionReference.Create(lookAction);
      }
   }


   /// <summary>
   /// Este metodos es para que input reader susbcribirse a actiones de este clase y para invokarlom
   /// si quirene ser mas optimada puede cambiarse que sea metodos con firma y susbribe en enable or disable,
   /// o tambien puede manejar bien la subscrition de otros method a la hora de subscribirse este clase
   /// </summary>
   private void InitReaderAction()
   {
      m_reader.Move += dir => Move?.Invoke(dir);
      m_reader.Look += dir => Look?.Invoke(dir);
      m_reader.InteractPressed +=  () => InteractPressed?.Invoke();
      m_reader.InteractReleased += () => InteractReleased?.Invoke();
      
      EnableReader(true);
   }
   
   private void EnableReader(bool enable) => m_readerEvent.Raise((m_reader,enable));


   public InputReader InputReader => m_reader;
   
  
}