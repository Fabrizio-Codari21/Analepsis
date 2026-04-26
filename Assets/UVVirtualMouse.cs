using UnityEngine;
using UnityEngine.InputSystem;

public class UVVirtualMouse : MonoBehaviour
{
    [Header("Refs")] public Camera uiCamera;

    [Header("State")] public bool useUV;
    public Vector2 uv;
    [SerializeField] private Vector2EventChannel uvPositionChannel;


    private Mouse virtualMouse;

    void OnEnable()
    {
        if (virtualMouse is not { added: true })
        {
            virtualMouse = InputSystem.AddDevice<Mouse>("UVVirtualMouse");
        }

        uvPositionChannel.OnEventRaised += OnUV;
    }

    void OnDisable()
    {

        uvPositionChannel.OnEventRaised -= OnUV;

        if (virtualMouse is { added: true })
        {
            InputSystem.QueueDeltaStateEvent(virtualMouse.leftButton, false);
        }
    }

    void OnDestroy()
    {
        if (virtualMouse is not { added: true }) return;
        InputSystem.RemoveDevice(virtualMouse);
        virtualMouse = null;
    }

    private void OnUV(Vector2 newUV)
    {
        uv = newUV;
        useUV = uv.x >= 0;
    }

    void Update()
    {
        if (virtualMouse is not { added: true } || uiCamera == null || !useUV)
        {
            if (virtualMouse != null)
                InputSystem.QueueDeltaStateEvent(virtualMouse.position, new Vector2(-1000, -1000));
            return;
        }
       
        Vector2 screenPos = uiCamera.ViewportToScreenPoint(new Vector2(uv.x, uv.y));
        

      
        bool pressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        Vector2 scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue() : Vector2.zero;
        
        InputSystem.QueueDeltaStateEvent(virtualMouse.position, screenPos);
        InputSystem.QueueDeltaStateEvent(virtualMouse.scroll, scroll);
        InputSystem.QueueDeltaStateEvent(virtualMouse.leftButton, pressed);


        InputSystem.Update();
    }
}