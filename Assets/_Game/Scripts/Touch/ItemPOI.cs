using UnityEngine;
using Random = UnityEngine.Random;

public class ItemPOI : POI
{
    public string poiId;
    private Renderer _renderer;
    private ItemViewer _viewer;
    
    
    private void Start()
    {
        _viewer = GetComponentInParent<ItemViewer>();
        _renderer =GetComponentInParent<Renderer>();
     
    }

  
    public override void Touch()
    {
        base.Touch();
        _viewer?.PoiReceived(poiId);
        _renderer.material.color = Random.ColorHSV(); // for debug

    }
}




