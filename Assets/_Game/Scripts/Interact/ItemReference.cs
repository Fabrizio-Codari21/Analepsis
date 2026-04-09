using UnityEngine;

public class ItemReference : MonoBehaviour
{
    [SerializeField] private Item m_itemReference;
    
    [SerializeField] private DynamicTextSetting m_nameTextSetting;
    [SerializeField] private Vector3 m_textPositionOffset;
    private IInteractable _interact;

    private ITipProvider _tipProvider;
    private DynamicText _text;
    [SerializeField] private RecordNoteEvent  m_recordNoteEvent;

    private void Start()
    {
        _interact = GetComponent<IInteractable>();
        _interact.OnFocus += SpawnName;
        _interact.OnUnfocus  += DespawnName;
        _interact.OnStart += DespawnName;
        _interact.OnStart += RecordItem;
        _tipProvider = GetComponent<ITipProvider>();
        _tipProvider.AddTip(new Tip("(INTERACTION)",TipOrder.InteractionType)); 
    }
    private void SpawnName()
    {
        _text = FlyweightFactory.Instance.Spawn<DynamicText>(m_nameTextSetting, m_textPositionOffset+transform.position,Quaternion.identity,transform);
        _text.SetText(m_itemReference.Name,1,Color.black);
        _ = _text.PlayTypeWriterEffect();
    }

    private void DespawnName()
    {
       if(_text) FlyweightFactory.Instance.Return(_text);
       _text =  null;
    }

    private void RecordItem()
    {
        m_recordNoteEvent.Raise(new ItemNote(m_itemReference.Name,m_itemReference));
    }
    
    
    public Item GetInspectItem()
    {
        return m_itemReference;
    }
    
    
    
    
    
}

