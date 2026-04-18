using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemViewer : MonoBehaviour
{
    [SerializeField] private Item m_item;

    private Dictionary<string,ItemPOIData> _containsData;

    
    public void PoiReceived(string poiId)
    {


      foreach (var poi in m_item.pois)
      {
          if (poi.poiId != poiId) continue;
          Debug.Log("Find");
          break;
      }
    }
}