using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class DraggableButton<T> : ButtonFactoryObject, IBeginDragHandler, IDragHandler, IEndDragHandler
{ 
    Transform _originalTransform;
    int _originalHierarchyPosition;
    private Canvas _canvas;
    
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvas = GetComponentInParent<Canvas>();
        
        if (!_canvas)
        {
            Debug.Log("Canvas not set");
            return;
        }


        if (transform.parent != null)
        {
            _originalTransform = transform.parent;
            _originalHierarchyPosition = transform.GetSiblingIndex();
        }
        transform.SetParent(_canvas.transform,false);
        MoveToLast();
        SetDraggedPosition(eventData);
       
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(m_button != null && m_button.interactable) SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if(result.gameObject == null || result.gameObject == gameObject) continue;
            if (!result.gameObject.TryGetComponent(out ISlotData<T> acceptor))
            {
                Debug.Log($" DraggleButton : no has slot");
                continue;
            }
            if (!acceptor.CanAccept())
            {
                Debug.LogWarning($"   DraggleButton : has slot but is block");
                continue;
            }
            
            T myData = GetButtonData();
            if (myData == null)
            {
                Debug.LogError($"   {gameObject.name}  GetButtonData() Return Null");
                continue;
            }

            
            if (!acceptor.CheckSlotAdapt(myData))
            {
                Debug.LogWarning($"  Diferente Type");
                continue;
            }
            
            if (!acceptor.ReplaceData(myData))
            {
                Debug.LogWarning($" Has Something");
                continue;
            }

            Insert(acceptor.SlotTransform);
            return;
        }

        if (_originalTransform == null) return;
        transform.position = _originalTransform.position;
        transform.rotation = _originalTransform.rotation;
        
        transform.SetParent(_originalTransform, true);
        transform.SetSiblingIndex(_originalHierarchyPosition);
    }
    
    protected abstract T GetButtonData();
    private void SetDraggedPosition(PointerEventData data)
    {
        if (_canvas == null) return;
        RectTransform draggingPlane = _canvas.transform as RectTransform;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out var globalMousePos)) return;
        m_rectTransform.position = globalMousePos;
        if (draggingPlane != null) m_rectTransform.rotation = draggingPlane.rotation;
    }
    
    
    
    protected virtual void Insert(Transform slotTransform)
    {
        transform.position = slotTransform.position;
        
        transform.rotation = slotTransform.rotation;
        
        transform.SetParent(slotTransform, true);
      
    }
}

public interface ISlotAcceptor
{
    Transform SlotTransform { get; }

    bool CanAccept();

    void ClearSlot();
}


public interface ISlotData<T> : ISlotAcceptor
{
    bool ReplaceData(T data);

    bool CheckSlotAdapt(T data);
}