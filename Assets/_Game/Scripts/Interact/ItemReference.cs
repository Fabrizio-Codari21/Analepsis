using UnityEngine;

public class ItemReference : MonoBehaviour
{
    [SerializeField] private Item m_itemReference;
    
    [SerializeField] private DynamicTextSetting m_nameTextSetting;
    [SerializeField] private Vector3 m_textPositionOffset;
    private IInteractable _focus;

    private ITipProvider _tipProvider;
    private DynamicText _text;

    private void Start()
    {
        _focus = GetComponent<IInteractable>();
        _focus.OnFocus += SpawnName;
        _focus.OnUnfocus  += DespawnName;
        _focus.OnStart += DespawnName;
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
    
    public Item GetInspectItem()
    {
        return m_itemReference;
    }
    
    
    
}

