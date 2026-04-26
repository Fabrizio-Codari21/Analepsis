using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableButton : ButtonFactoryObject, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform _originalTransform;
    int _originalHierarchyPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        var canvas = GetComponentInParent<Canvas>(); if(!canvas) return;
        if (!m_button.interactable) return;

        if(!_boardTransforms.ContainsValue(m_button.transform.parent.GetComponent<TheoryPanel>())) 
        {
            _originalTransform = m_button.transform.parent;
            _originalHierarchyPosition = m_button.transform.GetSiblingIndex();
        }
        m_button.transform.SetParent(canvas.transform, false); MoveToFirst();

        SetDraggedPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
       if(m_button != null && m_button.interactable) SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_button != null && m_button.interactable && eventData.pointerDrag != null)
        {
            InsertClue(eventData);
        }
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        RectTransform draggingPlane = canvas.transform as RectTransform;

        var rt = m_rectTransform;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position,
                data.pressEventCamera, out var globalMousePos)) return;
        rt.position = globalMousePos;
        if (draggingPlane != null) rt.rotation = draggingPlane.rotation;
    }

    void InsertClue(PointerEventData data)
    {
        //print("Dropped on " + data.pointerEnter.gameObject.name);

        TheoryPanel droppedOn = null;
        if (!data.pointerEnter.TryGetComponent(out droppedOn))
        {
            print("No panel found");
            m_button.transform.SetParent(_originalTransform, true);
            m_button.transform.SetSiblingIndex(_originalHierarchyPosition);
            return;
        }

        var panel = _boardTransforms.FirstOrDefault(x => x.Value == droppedOn);
        //if (panel.Value == default) print("no hay pruebas en " + m_text.text);

        if (droppedOn != null && _boardTransforms.ContainsValue(droppedOn) && (proof != null && proof.Contains(panel.Key)))
        {
            if (droppedOn.droppedClue != null) Destroy(droppedOn.droppedClue.gameObject);
            var button = _view.CreateClueButton(m_text.text, panel.Value.transform, proof);
            droppedOn.droppedClue = button;

            m_button.transform.SetParent(_originalTransform, true);
            m_button.transform.SetSiblingIndex(_originalHierarchyPosition);
            print($"You inserted: {m_text.text}.");

        }
        else
        {
            if (proof == null || !proof.Contains(panel.Key)) print("No valid proof list found: " + proof);
            else print("No panel found");

            m_button.transform.SetParent(_originalTransform, true);
            m_button.transform.SetSiblingIndex(_originalHierarchyPosition);
        }
    }




    
}
