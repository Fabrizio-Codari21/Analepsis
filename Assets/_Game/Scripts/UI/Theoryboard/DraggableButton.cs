using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DraggableButton : ButtonFactoryObject, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform _originalTransform;
    private int _originalHierarchyPosition;
    protected RectTransform _rectTransform;
    protected Canvas _canvas;


    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }
}


public class EvidenceRepresentButton : DraggableButton
{
   
}