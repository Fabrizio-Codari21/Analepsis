using System;
using UnityEngine;

public abstract class FactoryObject : MonoBehaviour, IFlyweight
{
    
    public event Action<IFlyweight> OnReleaseRequested;

    public virtual void OnSpawn()
    {
        gameObject.SetActive(true);   
    }
    public void OnDespawn()
    {
        OnReleaseRequested?.Invoke(this);
        gameObject.SetActive(false);
    }
    public void Free()
    {
        OnDespawn();
    }
    public virtual void SetPositionAndRotation(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        transform.position = pos;
        transform.rotation = rot;
        transform.SetParent(parent);
    }
}