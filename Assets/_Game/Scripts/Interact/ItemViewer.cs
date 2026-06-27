
using System.Linq;
using UnityEngine;

public class ItemViewer : MonoBehaviour
{
    [SerializeField] private Item m_item;
    [SerializeField] private StringEventChannel poiEventChannel;
    public void PoiReceived(string poiId)
    {
        if (m_item.pois.All(p => p.poiId != poiId)) return;
        bool isNewUnlock = NotebookManager.Instance.UnlockPoi(m_item, poiId);
        if (!isNewUnlock) return;
        var poiData = m_item.pois.Find(x => x.poiId == poiId);
        poiEventChannel.Raise(poiData.description);
    }
}