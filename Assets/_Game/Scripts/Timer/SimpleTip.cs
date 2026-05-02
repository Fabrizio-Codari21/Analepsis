using UnityEngine;

public class SimpleTip : MonoBehaviour
{
    private IInteractable  _tipProvider;
    private Tip _tip;

    [SerializeField] private string m_myTip;
    [SerializeField] private TipOrder m_tipOrder;
    [SerializeField] private bool _overrideFocus;

    private void Start()
    {
        _tipProvider = GetComponent<IInteractable>();
        _tip  = new Tip(m_myTip,m_tipOrder);
        _tipProvider.AddTip(_tip);
 
    }
    
    private void OnDestroy()
    {
        _tipProvider.RemoveTip(_tip);
    }
}