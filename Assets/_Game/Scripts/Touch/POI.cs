using System;
using UnityEngine;
/// <summary>
/// point of interest
/// </summary>
public class POI : MonoBehaviour,ITouch
{
    public event Action OnFocus;
    public event Action OnUnfocus;
    public virtual void Focus()
    {
       
    }

    public virtual void Unfocus()
    {
       
    }

    public virtual void Touch()
    {
        Debug.Log("Touch");
    }
}