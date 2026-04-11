using System;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableButton : ButtonFactoryObject, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform _originalTransform;

    public void OnBeginDrag(PointerEventData eventData)
    {
        var canvas = GetComponentInParent<Canvas>(); if(!canvas) return;

        if(m_button.transform.parent != _boardTransform) _originalTransform = m_button.transform.parent;
        m_button.transform.SetParent(canvas.transform, false); MoveToFirst();

        SetDraggedPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
       if(m_button != null) SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_button != null && eventData.pointerDrag != null)
        {
            InsertClue(eventData);
        }
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        RectTransform draggingPlane = default;
        if (data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
            draggingPlane = data.pointerEnter.transform as RectTransform;

        var rt = m_rectTransform;
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = draggingPlane.rotation;
        }
    }

    void InsertClue(PointerEventData data)
    {
        print("Dropped on " + data.pointerEnter.gameObject.name);
        Transform droppedOn = data.pointerEnter.transform ? data.pointerEnter.transform : null;
        if (droppedOn != null && (droppedOn == _boardTransform || droppedOn == _boardTransform.parent))
        {
            var button = _view.CreateClueButton(m_text.text, _boardTransform);

            m_button.transform.SetParent(_originalTransform, true);
            print($"You inserted: {m_text.text}.");
        }
        else
        {
            m_button.transform.SetParent(_originalTransform, true); //MoveToLast();
            return;
        }
    }




    void Start()
    {
        //m_text = m_rectTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
