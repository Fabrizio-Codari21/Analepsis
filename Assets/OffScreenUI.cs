using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OffScreenUI : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private Vector2EventChannel uvPositionChannel;
    [SerializeField] private Camera uiCamera; 

    [Header("Debug Setup")]
    [SerializeField] private RectTransform debugDot; 
    [SerializeField] private bool showDebugDot = true;

    private PointerEventData m_pointerData;
    private GameObject m_currentHovered;
    private GameObject m_currentPressed;

    private void OnEnable() => uvPositionChannel.OnEventRaised += HandleInteraction;
    private void OnDisable() => uvPositionChannel.OnEventRaised -= HandleInteraction;

    private void HandleInteraction(Vector2 uv)
    {
        if (EventSystem.current == null || uiCamera == null) return;
        m_pointerData ??= new PointerEventData(EventSystem.current);

        if (uv.x < 0) 
        { 
            if(debugDot != null) debugDot.gameObject.SetActive(false);
            ClearHover(); 
            return; 
        }

        
        float width = canvasRect.rect.width;
        float height = canvasRect.rect.height;
        
        if (debugDot != null)
        {
            debugDot.gameObject.SetActive(showDebugDot);
         
            debugDot.anchoredPosition = new Vector2((uv.x - 0.5f) * width, (uv.y - 0.5f) * height);
        }

      
        m_pointerData.position = new Vector2(uv.x * width, uv.y * height);

     
        m_pointerData.displayIndex = 0; 
        
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(m_pointerData, results);

        if (results.Count > 0)
        {
            ProcessEvents(results[0].gameObject);
        }
        else
        {
            ClearHover();
        }
    }

    private void ProcessEvents(GameObject hitObject) 
    {
        if (hitObject != m_currentHovered)
        {
            if (m_currentHovered != null) ExecuteEvents.Execute(m_currentHovered, m_pointerData, ExecuteEvents.pointerExitHandler);
            ExecuteEvents.Execute(hitObject, m_pointerData, ExecuteEvents.pointerEnterHandler);
            m_currentHovered = hitObject;
        }

        
        if (Input.GetMouseButtonDown(0))
        {
         
            GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject);
            if (handler != null)
            {
                m_currentPressed = handler;
                m_pointerData.pointerPress = handler;
                ExecuteEvents.Execute(m_currentPressed, m_pointerData, ExecuteEvents.pointerDownHandler);
             
            }
        }
        
        if (Input.GetMouseButton(0) && m_currentPressed != null)
        {
            ExecuteEvents.Execute(m_currentPressed, m_pointerData, ExecuteEvents.dragHandler);
        }
        
        if (Input.GetMouseButtonUp(0) && m_currentPressed != null)
        {
            ExecuteEvents.Execute(m_currentPressed, m_pointerData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(m_currentPressed, m_pointerData, ExecuteEvents.pointerClickHandler);
            m_currentPressed = null;
        }
    }

    private void ClearHover()
    {
        if (m_currentHovered != null)
        {
            ExecuteEvents.Execute(m_currentHovered, m_pointerData, ExecuteEvents.pointerExitHandler);
            m_currentHovered = null;
        }
    }
}