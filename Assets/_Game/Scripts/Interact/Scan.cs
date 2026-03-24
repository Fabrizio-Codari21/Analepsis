using UnityEngine;

public static class Scan<T>
{
    
    private static readonly Collider[] results =  new Collider[10];
    
    public static T ScanTypeForClosest(Transform origin,float range,LayerMask mask)
    {
        T result = default;
        float minDistance = float.MaxValue;
        var originPos = origin.position;
        var size = Physics.OverlapSphereNonAlloc(origin.position, range, results,mask);
        if (size <= 0) return default;

        for (int i = 0; i < size; i++)
        {
            if (!results[i].TryGetComponent(out T candidate)) continue;
            float dist = (originPos - results[i].transform.position).sqrMagnitude;
            if (dist >= minDistance) continue;
            minDistance = dist;
            result = candidate;
        }
        
        return result;
    }
}