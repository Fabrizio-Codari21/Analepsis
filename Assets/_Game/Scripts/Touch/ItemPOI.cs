using System;
using UnityEngine;

public class ItemPOI : POI
{
    public string poiId;
    
    private ItemViewer _viewer;
    private void Start()
    {
        _viewer = GetComponentInParent<ItemViewer>();
    }

  
    public override void Touch()
    {
        base.Touch();
        _viewer?.PoiReceived(poiId);
    }
}




public enum PoIState    
{
    Locked,
    Unlocked,
    Obtained
    
}