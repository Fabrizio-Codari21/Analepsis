
using System.Linq;
using UnityEngine;

public class ItemViewer : MonoBehaviour
{
    [SerializeField] private Item m_item;
    public void PoiReceived(string poiId)
    {
        if (m_item.pois.Any(p => p.poiId == poiId))
        {
            NotebookManager.Instance.UnlockPoi(m_item, poiId);
            
        }
    }
}