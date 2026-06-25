using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class RaycastPassThroughBlocker : MonoBehaviour, ICanvasRaycastFilter, IPointerDownHandler
{
    public Action OnClickedOutside;
    
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return false; 
    }

  
    public void OnPointerDown(PointerEventData eventData)
    {
        OnClickedOutside?.Invoke();
    }
}