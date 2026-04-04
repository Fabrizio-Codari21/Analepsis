using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class InteractView : MonoBehaviour
{
    [SerializeField] private InteractableFocusEventChannel m_focusEvent;
    [SerializeField] private InteractableFocusEventChannel m_unfocusEvent;
    
    [SerializeField] private DynamicTextSetting m_dynamicTextSetting;
    [SerializeField] private Transform m_textCanvaRoot;
    [SerializeField] private Transform m_textRoot;
    
    private CancellationTokenSource _cts;
    private void OnEnable()
    {
        m_focusEvent.OnEventRaised += Show;
        m_unfocusEvent.OnEventRaised += Hide;
    }

    private void OnDisable()
    {
        m_focusEvent.OnEventRaised -= Show;
        m_unfocusEvent.OnEventRaised -= Hide;
        CancelCurrent();
    }
    
    private  void Show(IInteractable interactable)
    {
        
            m_textCanvaRoot.gameObject.SetActive(true);
            CancelCurrent();
            _cts = new CancellationTokenSource();
      
            _ =  UIElement.PlayDynamicText(
                interactable.GetTip(),
                m_dynamicTextSetting,
                Vector3.zero,
                Quaternion.identity,
                m_textRoot,
                _cts.Token);
       
       
    }

    private void Hide(IInteractable interactable)
    {
        CancelCurrent();
      
        m_textCanvaRoot.gameObject.SetActive(false);
  
    }
    
    private void CancelCurrent()
    {
        if (_cts == null) return;

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;

        foreach (var f in m_textRoot.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
        
    }
}