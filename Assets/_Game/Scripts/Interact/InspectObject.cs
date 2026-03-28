using System;
using UnityEngine;

public class InspectObject : MonoBehaviour,IInspectable
{
    
    private Interactable _interactable;
    
    [SerializeField] private Item m_itemReference;
    [SerializeField] private InspectableEvent m_event;
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();

        _interactable.OnEnd += Inspect;
    }
    
    private void Inspect()
    {
        m_event.Raise(this);
    }


    public Item GetInspectItem()
    {
       return m_itemReference;
    }
}