using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;

public class TheoryboardManager : MonoBehaviour, IActivity
{
    
    [Header("Core")]
    [SerializeField] private TheoryboardView m_view;
    [SerializeField] private BoardInputReader inputReaderBoard;
    #region Event
    [Header("Events")]
    
    [Header("Open & Close")]
    [SerializeField] private EventChannel m_openTheoryBoardChannel;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
    
    [Header("Core Event")]
    [SerializeField] private EventChannel m_solveCaseEvent;
    #endregion
    [Header("Data")] 
    [Space(5)]
    [Header("Life")]
    [ShowInInspector, ReadOnly] private int _leftAttempts;
    [SerializeField] private int m_maxSolveAttempts;
    [Header("Case")]
    [SerializeField] private CaseResolution m_caseResolution;
    [Header("Text Base")] 
    [SerializeField, TextArea(3, 10)]
    private string m_baseTextOnTrySolver;
    [Header("Test Panel")]
    [SerializeField] private bool isInstaWin = false;
    
    private List<TheorySlot> _cachedSlotsInScene = new List<TheorySlot>();
    

    #region IActivity
    private void Open() => pushEvent.Raise(this);
    private void Close() => popEvent.Raise();
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public bool CanPopWithKey()
    {
        return true;
    }

    public void Pause()
    {

        OnPause?.Invoke();
        inputReaderBoard.SetEnable(false);
        enableCursor.Raise(false);
        
    }

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderBoard.SetEnable();
        enableCursor.Raise(true);
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }
    #endregion

    #region Unity Life
    
    private void Start()
    {

        m_view = Instantiate(m_view,transform);
        _cachedSlotsInScene = m_view.InitializeBoardArchitecture(m_caseResolution);
        m_openTheoryBoardChannel.OnEventRaised += Open;
        m_openTheoryBoardChannel.OnEventRaised += m_view.LoadMarkedClues;
        inputReaderBoard.Close += Close;
        
        m_solveCaseEvent.OnEventRaised += SolveCase;
        
        _leftAttempts = m_maxSolveAttempts;
        
    }

    private void OnDestroy()
    {
        m_openTheoryBoardChannel.OnEventRaised -= Open;
        m_openTheoryBoardChannel.OnEventRaised -= m_view.LoadMarkedClues;
        inputReaderBoard.Close -= Close;
        m_solveCaseEvent.OnEventRaised -= SolveCase;
        
    }

    #endregion


    private void SolveCase()
    {
        if (isInstaWin)
        {
            TryResult(true);
            return;
        }
        TryResult(TrySolveCase());
    }
 

    private bool TrySolveCase()
    {
        if (m_caseResolution == null) return false;
        
        CaseAnswer resultAnswer = m_caseResolution.ValidateCase(_cachedSlotsInScene);

        if (resultAnswer == null) return false;
        CaseSuccess();
        return true;

    }

    private void TryResult(bool success)
    {
        if (success)
        {
            CaseSuccess();
            return;
        }
        
        CaseFail();
    }

    private void CaseSuccess()
    {
        
    }

    private void CaseFail()
    {
        _leftAttempts--;
        
        if (_leftAttempts <= 0)
        {
            Lose();
            return;
        }
        m_view.Tip(m_baseTextOnTrySolver + $"\n [{_leftAttempts} attempts left]").Forget();
       
    }
    
    private void Lose()
    {
        
    }
    
}




public enum Whodunnit
{
    NoProof,
    Victim,
    Killer,
    Motive,
    Weapon,
    Place,
}
