using UnityEngine;

public abstract class FlyweightSetting : ScriptableObject
{
    public GameObject prefab;
    public int defaultCapacity = 10;
    public int maxPoolSize = 100;
    public virtual IFlyweight Create()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab Not Found");
            return null;
        }
        if (!prefab.TryGetComponent<IFlyweight>(out _))
        {
            Debug.LogError("Prefab Not is IFlyweightFactory");
            return null;
        }
        var go = Instantiate(prefab);
        go.SetActive(false);
        var fw = go.GetComponent<IFlyweight>();
        return fw;
    }
    public virtual void OnGet(IFlyweight f)
    {
        f.OnSpawn();
    }

    public virtual void OnRelease(IFlyweight f)
    {
      f.OnDespawn();  
    }
}