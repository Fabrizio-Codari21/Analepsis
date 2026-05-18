using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverDetector : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public bool IsMouseHovering { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsMouseHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsMouseHovering = false;
    }

    private void OnDisable()
    {
        IsMouseHovering = false;
    }
}