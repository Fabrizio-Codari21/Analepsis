using System.Collections.Generic;
using UnityEngine;

public class TransformKeyManager : Singleton<TransformKeyManager>
{
    
    private readonly Dictionary<TransformKey, TransformKeyInstance> keyInstances = new Dictionary<TransformKey, TransformKeyInstance>();

    public void Register(TransformKey key, TransformKeyInstance instance)
    {
        if (keyInstances.TryAdd(key, instance)) return;
        Debug.LogError($"Duplicate TransformKey: {key.name}", instance);
    }
   
    public void Unregister(TransformKey key, TransformKeyInstance instance)
    {
        if (keyInstances.TryGetValue(key, out var current) && current == instance)
        {
            keyInstances.Remove(key);
        }
    }

    public Transform GetTransform(TransformKey key)
    {
        return !keyInstances.TryGetValue(key, out var keyInstance) ? null : keyInstance.transform;
    }
   
}