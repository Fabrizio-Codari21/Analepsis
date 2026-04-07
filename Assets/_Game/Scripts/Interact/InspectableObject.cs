using UnityEngine;
[RequireComponent(typeof(ItemReference))]
public class InspectableObject : MonoBehaviour,IInspectable
{
    private Interactable _interactable;
    [SerializeField] private ItemReference  m_itemReference;
    [SerializeField] private InspectableEvent m_event;
   
    private void Awake()
    {
        _interactable = GetComponent<Interactable>();

        _interactable.OnStart += Inspect;
    }
    
    private void Inspect()
    {
        m_event.Raise(this);
    }

    public Item GetInspectItem()
    {
       return m_itemReference.GetInspectItem();
    }
}