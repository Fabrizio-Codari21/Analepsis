
using UnityEngine;
using PrimeTween;

public class CursorManager : PersistentSingleton<CursorManager>
{
    [SerializeField] private BoolEventChannel m_cursorEnableChannel;
    [SerializeField] private CursorAnimationEvent m_cursorAnimationChannel;
    [SerializeField] private CursorAnimation m_defaultCursorAnimation;
    
    private bool _cursorEnabled;
    
    private Sequence _sequence = default;
    private void Start()
    {
        m_cursorEnableChannel.OnEventRaised += CursorEnable;
        m_cursorAnimationChannel.OnEventRaised += PlayCursor;

        CursorEnable(false);
    }
    private void CursorEnable(bool enable)
    {
        CursorLockMode targetMode = enable ? CursorLockMode.None : CursorLockMode.Locked;
        if (Cursor.lockState == targetMode && Cursor.visible == enable) return;
        Cursor.lockState = targetMode;
        Cursor.visible = enable;
    }
    private void PlayCursor(CursorAnimation ca )
    {
        if(_sequence.isAlive) _sequence.Stop();
        
        if(ca.animationSheets.Length <= 0) return;
        if (ca.animationSheets.Length == 1)
        {
            Cursor.SetCursor(ca.animationSheets[0], Vector2.zero, CursorMode.Auto);
            return;
        }
        _sequence = Sequence.Create(cycles: -1);
        float frameTime = 1f / ca.frameRate;
        foreach (var t in ca.animationSheets)
        {
            var frame = t;
            _sequence.ChainCallback(() => { Cursor.SetCursor(frame, Vector2.zero, CursorMode.Auto); });
            _sequence.ChainDelay(frameTime);
        }
    }

    private void OnDestroy()
    {
        m_cursorEnableChannel.OnEventRaised -= CursorEnable;
        m_cursorAnimationChannel.OnEventRaised -= PlayCursor;
    }
    
}