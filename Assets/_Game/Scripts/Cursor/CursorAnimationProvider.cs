using UnityEngine;

public class CursorAnimationProvider : MonoBehaviour
{
    private IActivity _activity;

    [SerializeField] private EventChannel m_resetCursor;
    [SerializeField] private CursorEvent m_animationEvent;
    [SerializeField] private CustomCursor m_animation2D;
    private void Awake()
    {
        _activity = GetComponentInParent<IActivity>();

        _activity.OnResume += PushCursor;
        
        _activity.OnPause += PopCursor;
    }

    private void OnDestroy()
    {
        _activity.OnResume -= PushCursor;
        _activity.OnPause -= PopCursor;
    }

    private void PushCursor() => m_animationEvent.Raise(m_animation2D);
    
    private void PopCursor() => m_resetCursor.Raise();



}