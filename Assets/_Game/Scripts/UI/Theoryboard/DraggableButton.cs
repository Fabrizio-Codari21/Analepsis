using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class DraggableButton : ButtonFactoryObject, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform _originalTransform;
    private int _originalHierarchyPosition;
    protected RectTransform _rectTransform;
    protected Canvas _canvas;

    // Ghost 相关变量
    private GameObject _ghostObject;
    private RectTransform _ghostRect;

    protected bool IsInteractable => m_button != null && m_button.interactable;

    protected virtual void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvas = GetComponentInParent<Canvas>(); 
        if (!_canvas || !IsInteractable) return;
        
        // 1. 记录原本的父级和位置（用于失败回弹）
        _originalTransform = transform.parent;
        _originalHierarchyPosition = transform.GetSiblingIndex();
        
        // 2. 创建 Ghost（影子）
        CreateGhost();

        // 3. 隐藏本体的视觉流（让本体在原位变透明，或者直接隐藏）
        // 注意：不要直接 SetActive(false)，否则会中断 UGUI 的 Drag 事件流！
        SetVisibility(false);

        // 4. 初始化 Ghost 位置
        UpdateGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsInteractable && _ghostObject != null) 
        {
            UpdateGhostPosition(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 显示本体
        SetVisibility(true);

        if (IsInteractable && eventData.pointerDrag != null)
        {
            ProcessDrop(eventData);
        }

        // 无论成功与否，最后销毁 Ghost
        DestroyGhost();
    }

    private void CreateGhost()
    {
        // 实例化一个自己的副本作为 Ghost
        _ghostObject = Instantiate(this.gameObject, _canvas.transform, false);
        _ghostRect = _ghostObject.GetComponent<RectTransform>();

        // 关键：移除 Ghost 身上所有的脚本和交互组件，只留下视觉（Image/Text）
        // 这样 Ghost 就绝对不会阻挡系统的 Raycast 射线
        var scripts = _ghostObject.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            Destroy(script);
        }
        
        // 确保 Ghost 的射线检测是关闭的
        if (_ghostObject.TryGetComponent<Graphic>(out var graphic))
        {
            graphic.raycastTarget = false;
        }
        var childGraphics = _ghostObject.GetComponentsInChildren<Graphic>();
        foreach (var cg in childGraphics) cg.raycastTarget = false;

        // 调整 Ghost 的尺寸与本体一致
        _ghostRect.sizeDelta = _rectTransform.sizeDelta;
    }

    private void UpdateGhostPosition(PointerEventData data)
    {
        if (_canvas == null || _ghostRect == null) return;
        RectTransform draggingPlane = _canvas.transform as RectTransform;

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out var globalMousePos)) return;
        _ghostRect.position = globalMousePos;
    }

    private void ProcessDrop(PointerEventData data)
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, raycastResults);

        bool isSuccessfullyPlaced = false;

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject == null || result.gameObject == this.gameObject) continue;

            ISlotAcceptor slot = result.gameObject.GetComponent<ISlotAcceptor>();
            if (slot != null)
            {
                isSuccessfullyPlaced = HandlePlacement(slot);
                if (isSuccessfullyPlaced) break;
            }
        }

        // 如果失败，回弹原位
        if (!isSuccessfullyPlaced)
        {
            ReturnToOriginalPosition();
        }
    }

    private void SetVisibility(bool visible)
    {
      
        if (!TryGetComponent<CanvasGroup>(out var group))
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
        group.alpha = visible ? 1f : 0f;
    }

    protected abstract bool HandlePlacement(ISlotAcceptor slot);

    public void ReturnToOriginalPosition()
    {
        transform.SetParent(_originalTransform, false);
        transform.SetSiblingIndex(_originalHierarchyPosition);
        _rectTransform.anchoredPosition = Vector2.zero;
    }

    private void DestroyGhost()
    {
        if (_ghostObject != null)
        {
            Destroy(_ghostObject);
            _ghostObject = null;
        }
    }
}


public interface ISlotAcceptor
{
    Transform SlotTransform { get; }
}