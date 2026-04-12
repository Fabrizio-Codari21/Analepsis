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

        TheoryPanel droppedOn = data.pointerEnter.GetComponent<TheoryPanel>();
        var panel = _boardTransforms.Where(x => x.Value == droppedOn.transform || x.Value == droppedOn.transform.parent).FirstOrDefault();

        if (droppedOn != null
            && (_boardTransforms.ContainsValue(droppedOn))
            && (proof != default && proof.Contains(panel.Key)))
        {
            if (droppedOn.droppedClue != default) Destroy(droppedOn.droppedClue.gameObject);

            var button = _view.CreateClueButton(m_text.text, panel.Value.transform, proof);
            droppedOn.droppedClue = button;

            m_button.transform.SetParent(_originalTransform, true);
            m_button.transform.SetSiblingIndex(_originalHierarchyPosition);
            print($"You inserted: {m_text.text}.");

        }
        else
        {
            if (proof == default || !proof.Contains(panel.Key)) print("No valid proof list found: " + proof);
            else print("No panel found");

            m_button.transform.SetParent(_originalTransform, true);
            m_button.transform.SetSiblingIndex(_originalHierarchyPosition);
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
