using System;
using UnityEngine;
/// <summary>
/// point of interest
/// </summary>
public class POI : MonoBehaviour,ITouch
{
    public event Action OnFocus;
    public event Action OnUnfocus;
    public event Action<float> OnUpdateDistance;

    public virtual void Focus()
    {
    
       OnFocus?.Invoke();
    }

    public virtual void Unfocus()
    {
        OnUnfocus?.Invoke();
    }

    public virtual void Touch()
    {
        Debug.Log("Touch");
    }

    public void UpdateDistance(float dist)
    {
        throw new NotImplementedException();
    }
}