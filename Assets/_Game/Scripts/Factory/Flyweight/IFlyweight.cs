using System;
using UnityEngine;

public interface IFlyweight: ITransformable
{
    
    void OnSpawn();  
    void Despawn();
    void Free();
}

public interface ITransformable
{
    void SetPositionAndRotation(Vector3 pos, Quaternion rot,Transform parent = null);
}
