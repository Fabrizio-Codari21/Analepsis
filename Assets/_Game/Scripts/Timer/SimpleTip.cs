using UnityEngine;

public class SimpleTip : MonoBehaviour
{
    private ITipProvider  _tipProvider;
    private Tip _tip;

    [SerializeField] private string m_myTip;
    [SerializeField] private TipOrder m_tipOrder;

    private void Start()
    {
        _tipProvider = GetComponent<ITipProvider>();
        _tip  = new Tip(m_myTip,m_tipOrder);
        _tipProvider.AddTip(_tip);
    }
    
    private void OnDestroy()
    {
        _tipProvider.RemoveTip(_tip);
    }
}