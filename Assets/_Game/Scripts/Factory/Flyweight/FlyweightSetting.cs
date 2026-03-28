using UnityEngine;

public abstract class FlyweightSetting : ScriptableObject
{
    public GameObject prefab;
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
        fw.Setting = this;
        return fw;
    }

    public virtual void OnGet(IFlyweight f)
    {
        f.GO.SetActive(true);
    } 
    
    public virtual void OnRelease(IFlyweight  f) => f.GO.SetActive(false);
    
    public virtual void OnDestroyPoolObject(IFlyweight f) => Destroy(f.GO);
}