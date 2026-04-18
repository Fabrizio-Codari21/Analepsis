using System;
using Unity.Cinemachine;
using UnityEngine;

public class StaticCamera : MonoBehaviour,IActivity,ITouch
{
   [SerializeField] private CinemachineCamera m_camera;
   [SerializeField] private bool m_canPopWithKey;
   [SerializeField] private Collider myCollider;
   [SerializeField] private IActivityEvent m_pushEvent;
   public event Action OnResume;
   public event Action OnPause;
   public event Action OnStop;
   public void Resume()
   {
      m_camera.enabled = true;
      myCollider.enabled = false;
      OnResume?.Invoke();
   }

   public void Pause()
   {
      m_camera.enabled = false;
      myCollider.enabled = true;
      OnPause?.Invoke();
   }

   public void Stop()
   {
      Pause();
     OnStop?.Invoke();
   }

   public bool CanPopWithKey()
   {
      return m_canPopWithKey;
   }

   public event Action OnFocus;
   public event Action OnUnfocus;
   public void Focus()
   {
     Debug.Log("Focus , NEED EFFECT ON FOCUS");
     OnFocus?.Invoke();
   }
   public void Unfocus()
   {
      Debug.Log("UnFocus , NEED EFFECT ON UNFOCUS OR REMOVE EFFECT FOCUSD");
      OnUnfocus?.Invoke();
   }

   public void Touch()
   {
      m_pushEvent?.Raise(this);
   }
}
