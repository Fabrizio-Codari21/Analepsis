using System;
using UnityEngine;

public interface IFlyweight: ITransformable
{

    public event Action<IFlyweight> OnReleaseRequested; 
    void OnSpawn();  
    void OnDespawn();
    void Free();
}

public interface ITransformable
{
    void SetPositionAndRotation(Vector3 pos, Quaternion rot,Transform parent = null);
}
