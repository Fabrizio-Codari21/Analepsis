using System;
using UnityEngine;
/// <summary>
/// point of interest
/// </summary>
public class POI : MonoBehaviour,ITouch
{
    public event Action OnFocus;
    public event Action OnUnfocus;
    public void Focus()
    {
       
    }

    public void Unfocus()
    {
       
    }

    public void Touch()
    {
        Debug.Log("Touch");
    }
}
