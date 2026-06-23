using System;
using UnityEngine;

public abstract class FactoryObject : MonoBehaviour, IFlyweight
{
    public Action OnCleanUp;
    public virtual void OnSpawn()
    {
        gameObject.SetActive(true);  
    }
    public virtual void Despawn()
    {
        OnCleanUp?.Invoke();
        OnCleanUp = null;
        gameObject.SetActive(false);
    }
    public virtual void Free()
    {
        Destroy(gameObject);
    }
    public virtual void SetPositionAndRotation(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        transform.position = pos;
        transform.rotation = rot;
        transform.SetParent(parent);
    }
}