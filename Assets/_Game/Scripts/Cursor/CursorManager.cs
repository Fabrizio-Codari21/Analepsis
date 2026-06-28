using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField] private BoolEventChannel m_cursorEnableChannel;
    [Header("Event")]
    [InfoBox("Change Cursor")]
    [SerializeField] private CursorEvent m_cursorEventChannel; 
    [SerializeField] private CursorTexture m_cursorTextureDefault;
    [SerializeField] private EventChannel m_resetCursorChannel;

    [SerializeField] private bool m_visibleOnStart = true;
    private CursorTexture _currentCursorTexture;
    private Sequence _sequence = default;
    
    private enum CursorState { Up, TransitionToDown, Down, TransitionToUp }
    private CursorState _currentState = CursorState.Up;

    private void Start()
    {
        m_cursorEnableChannel.OnEventRaised += CursorEnable;
        ChangeCursorAsset(m_cursorTextureDefault);
        CursorEnable(m_visibleOnStart);
        m_cursorEventChannel.OnEventRaised += ChangeCursorAsset;
        m_resetCursorChannel.OnEventRaised += ResetCursor;
    }

    
    private void ResetCursor() => ChangeCursorAsset(m_cursorTextureDefault);
   

    private void OnDestroy()
    {
        m_resetCursorChannel.OnEventRaised -= ResetCursor;
        if (m_cursorEventChannel != null) m_cursorEventChannel.OnEventRaised -= ChangeCursorAsset;
        if (m_cursorEnableChannel != null) m_cursorEnableChannel.OnEventRaised -= CursorEnable;
        if (_sequence.isAlive) _sequence.Stop();
    }

    private void Update()
    {
        if (!Cursor.visible || Cursor.lockState == CursorLockMode.Locked) return;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlayTransitionState(toRelease: false);
        }
     
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            PlayTransitionState(toRelease: true);
        }
    }

    private void CursorEnable(bool enable)
    {
        CursorLockMode targetMode = enable ? CursorLockMode.None : CursorLockMode.Locked;
        if (Cursor.lockState == targetMode && Cursor.visible == enable) return;
        Cursor.lockState = targetMode;
        Cursor.visible = enable;
    }
    
    private void ChangeCursorAsset(CursorTexture newAsset)
    {
        if (newAsset == null) return;
        _currentCursorTexture = newAsset;
        PlayLoopState(CursorState.Up, forceReplay: true);
    }

 
    private void PlayTransitionState(bool toRelease)
    {
        if (_currentCursorTexture == null) return;
        if (_sequence.isAlive) _sequence.Stop();
        
        Texture2D[] transitionSheets = toRelease ? _currentCursorTexture.transitionToUp : _currentCursorTexture.transitionToDown;
        
        if (transitionSheets == null || transitionSheets.Length == 0)
        {
            PlayLoopState(toRelease ? CursorState.Up : CursorState.Down, forceReplay: true);
            return;
        }

        _currentState = toRelease ? CursorState.TransitionToUp : CursorState.TransitionToDown;
        
        _sequence = Sequence.Create(cycles: 1);
        float frameTime = 1f / _currentCursorTexture.frameRate;

        foreach (var tex in transitionSheets)
        {
            var currentTex = tex; 
            var hotSpot = _currentCursorTexture.m_skewedVector;

            _sequence.ChainCallback(() => { Cursor.SetCursor(currentTex, hotSpot, CursorMode.ForceSoftware); });
            _sequence.ChainDelay(frameTime);
        }

    
        _sequence.ChainCallback(() =>
        {
            PlayLoopState(toRelease ? CursorState.Up : CursorState.Down, forceReplay: true);
        });
    }
    

    private void PlayLoopState(CursorState state, bool forceReplay = false)
    {
        if (_currentState == state && !forceReplay) return;
        _currentState = state;

        if (_sequence.isAlive) _sequence.Stop();
        if (_currentCursorTexture == null) return;
        Texture2D[] targetSheets = (state == CursorState.Up) ? _currentCursorTexture.animationSheetsUp : _currentCursorTexture.animationSheetsDown;
        
      
        if (targetSheets == null || targetSheets.Length == 0) targetSheets = _currentCursorTexture.animationSheetsUp;
        if (targetSheets == null || targetSheets.Length == 0) return;

   
        if (targetSheets.Length == 1)
        {
            Cursor.SetCursor(targetSheets[0], _currentCursorTexture.m_skewedVector, CursorMode.ForceSoftware);
            return;
        }
        
        _sequence = Sequence.Create(cycles: -1);
        float frameTime = 1f / _currentCursorTexture.frameRate;

        foreach (var tex in targetSheets)
        {
            var currentTex = tex; 
            var hotSpot = _currentCursorTexture.m_skewedVector;

            _sequence.ChainCallback(() => { Cursor.SetCursor(currentTex, hotSpot, CursorMode.ForceSoftware); });
            _sequence.ChainDelay(frameTime);
        }
    }
}