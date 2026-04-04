using System;
using UnityEngine;

public class InspectableObject : MonoBehaviour,IInspectable
{
        private Interactable _interactable;
    
    [SerializeField] private Item m_itemReference;
    [SerializeField] private InspectableEvent m_event;
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();

        _interactable.OnStart += Inspect;
    }
    
    public void Inspect()
    {
        m_event.Raise(this);
    }

    public Item GetInspectItem()
    {
       return m_itemReference;
    }
}