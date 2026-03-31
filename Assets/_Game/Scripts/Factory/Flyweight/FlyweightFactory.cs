using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FlyweightFactory : Singleton<FlyweightFactory>
{
  
    private readonly Dictionary<FlyweightSetting, IObjectPool<IFlyweight>> _pools = new();
    private readonly Dictionary<IFlyweight, IObjectPool<IFlyweight>> _registry = new();
    private  IObjectPool<IFlyweight> GetPoolFor(FlyweightSetting setting)
    {
        if (_pools.TryGetValue(setting, out var pool)) return pool;
        pool = new ObjectPool<IFlyweight>(
            () =>
            {
                var fw = setting.Create();
                fw.OnReleaseRequested += Return;
                return fw;
            },
            (f) =>
            {
                _registry[f] = GetPoolFor(setting);
                setting.OnGet(f);
            } ,
            setting.OnRelease,
            f=> {
                f.OnReleaseRequested -= Return; 
                _registry.Remove(f);
                f.Free();
            },
            true,
            setting.defaultCapacity,
            setting.maxPoolSize);
        
        _pools.Add(setting, pool);
        return pool;
    }
    
    public T Spawn<T>(FlyweightSetting setting, Vector3 position, Quaternion rotation,Transform parent = null) where T : IFlyweight
    {
        var pool = GetPoolFor(setting);
        var flyweight =  pool?.Get();
        if(flyweight == null) return default;
        flyweight.SetPositionAndRotation(position, rotation,parent);
        return (T)flyweight;
    }
    private void Return(IFlyweight f)
    {
        if (f == null) return;
        if (_registry.TryGetValue(f, out var pool)) pool.Release(f);
        else f.Free();
        
    }
    public  void Clear()
    {
        foreach (var pool in _pools.Values) pool.Clear();
        _pools.Clear();
        _registry.Clear();
    }

    
}