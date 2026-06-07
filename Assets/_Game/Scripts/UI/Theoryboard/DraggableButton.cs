using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public abstract class DraggableButton<T> : ButtonFactoryObject, IBeginDragHandler, IDragHandler, IEndDragHandler
{ 
    private Transform _originalTransform;
    private int _originalHierarchyPosition;
    private Canvas _canvas;
    private T _data;
    private ISlotData<T> _myCurrentDataBase;
    public void InitData(T data, ISlotData<T> myCurrentBase)
    {
        _data = data;
        _myCurrentDataBase = myCurrentBase;
    }
    public T GetData() => _data;
    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvas = GetComponentInParent<Canvas>();
        if (!_canvas) return;

        if (transform.parent != null)
        {
            _originalTransform = transform.parent;
            _originalHierarchyPosition = transform.GetSiblingIndex();
        }

        transform.SetParent(_canvas.transform, false);
        MoveToLast();
        SetDraggedPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(m_button != null && m_button.interactable) SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ISlotData<T> targetSlot = GetTargetSlot(eventData);

        
        if (targetSlot == null)
        {
            if (_myCurrentDataBase is { ClearOnRemove: true }) _myCurrentDataBase.RemoveData(_data);
            
            else ReturnToOriginalPosition();
            
            return;
        }
        
        if (targetSlot == _myCurrentDataBase || !targetSlot.CheckSlotAdapt(_data) || !targetSlot.ReplaceData(_data))
        {
            ReturnToOriginalPosition();
            return;
        }

        if (_myCurrentDataBase is { ClearOnRemove: true })
        {
            _myCurrentDataBase.RemoveData(_data);
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }
    private void ReturnToOriginalPosition()
    {
        if (_originalTransform == null) return;
        transform.position = _originalTransform.position;
        transform.rotation = _originalTransform.rotation;
        transform.SetParent(_originalTransform, true);
        transform.SetSiblingIndex(_originalHierarchyPosition);
    }


    private void SetDraggedPosition(PointerEventData data)
    {
        if (_canvas == null) return;
        RectTransform draggingPlane = _canvas.transform as RectTransform;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out var globalMousePos)) return;
        m_rectTransform.position = globalMousePos;
        if (draggingPlane != null) m_rectTransform.rotation = draggingPlane.rotation;
    }
    
    private ISlotData<T> GetTargetSlot(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == null || result.gameObject == gameObject) continue;
            if (result.gameObject.TryGetComponent(out ISlotData<T> acceptor))
            {
                return acceptor;
            }
        }
        return null;
    }
}

public interface ISlotData<T>
{
    bool CheckSlotAdapt(T data);
    
    bool ReplaceData(T data);
    void ClearSlot();
    
    bool ClearOnRemove { get; }
    void RemoveData(T data);
    


}