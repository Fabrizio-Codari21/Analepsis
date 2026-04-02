using System;
using UnityEngine;

public abstract class FactoryObject : MonoBehaviour, IFlyweight
{
    
    public virtual void OnSpawn()
    {
        gameObject.SetActive(true);  
        Debug.Log("OnSpawn");
    }
    public virtual void Despawn()
    {
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