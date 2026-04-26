using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OffScreenUI : MonoBehaviour
{
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Vector2EventChannel uvPositionChannel;

    private PointerEventData m_pointerData;
    private EventSystem m_eventSystem;

    private void Awake()
    {
        m_eventSystem = EventSystem.current;
    }

    private void OnEnable() => uvPositionChannel.OnEventRaised += HandleInteraction;
    private void OnDisable() => uvPositionChannel.OnEventRaised -= HandleInteraction;

    private void HandleInteraction(Vector2 uv)
    {
        if (m_eventSystem == null || uiCamera == null)
            return;
        
        
        if (uv.x < 0)
        {
            m_eventSystem.SetSelectedGameObject(null);
            return;
        }

        m_pointerData ??= new PointerEventData(m_eventSystem);
        
        var rt = uiCamera.targetTexture;
        Vector2 screenPos = new Vector2(
            uv.x * rt.width,
            uv.y * rt.height
        );
        
        m_pointerData.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        m_eventSystem.RaycastAll(m_pointerData, results);

        if (results.Count > 0)
        {
            var hit = results[0].gameObject;

          
            
        }
        else
        {
          
        }
    }
}