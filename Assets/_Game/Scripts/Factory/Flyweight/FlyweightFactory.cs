using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class FlyweightFactory
{
    private static int defaultCapacity = 10;
    private static int maxCapacity = 100;
    private static Dictionary<FlyweightSetting, IObjectPool<IFlyweight>> pools = new();
    private static IObjectPool<IFlyweight> GetPoolFor(FlyweightSetting setting)
    {
        if (pools.TryGetValue(setting, out var pool)) return pool;
        pool = new ObjectPool<IFlyweight>(
            setting.Create,
            setting.OnGet,
            setting.OnRelease,
            setting.OnDestroyPoolObject,
            true,
            defaultCapacity,
            maxCapacity);
        
        pools.Add(setting, pool);
        return pool;
    }

    public static IFlyweight Spawn(FlyweightSetting setting) => GetPoolFor(setting).Get();
    

    public static IFlyweight Spawn(FlyweightSetting setting, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var pool = GetPoolFor(setting);
        var poolObject = pool?.Get();
        if (poolObject == null) return null;

        var t = poolObject.GO.transform;
        
        if(parent != null) t.SetParent(parent,false);
        t.position = position;
        t.rotation = rotation;
        t.localScale = Vector3.one;
        
        return poolObject;
    }
    public static void Return(IFlyweight f)
    {
        if (f == null || f.Setting == null) return;
        if (pools.TryGetValue(f.Setting, out var pool))
        {
            pool.Release(f);
            return;
        }
        
        pool = new ObjectPool<IFlyweight>(
            f.Setting.Create,
            f.Setting.OnGet,
            f.Setting.OnRelease,
            f.Setting.OnDestroyPoolObject,
            true,
            defaultCapacity,
            maxCapacity);
        
        pools.Add(f.Setting, pool);
        pool.Release(f);
       
    }

    public static void Clear(FlyweightSetting setting)
    {
        if (pools.TryGetValue(setting, out var pool)) pool.Clear();
    }

    
}