using Sirenix.OdinInspector;
using System;
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
    [ShowInInspector, ReadOnly] private int _leftAttempts;
    [SerializeField] private int m_maxSolveAttempts;
    
    [Header("Test Panel")]
    [SerializeField] private bool isInstaWin = false;



    

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
        // el check debe ser por aca
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
        
        // llamar a view
        
        if (_leftAttempts <= 0) Lose();
    }
    
    private void Lose()
    {
        
    }

    public async UniTask SolveCase(int answerID = 0, string answerName = "")
    {
        
        await this.AsyncLoader("WinScene");
        WinManager.Instance.SetConclusion(answerID);
    }
    public async UniTask FailCase()
    {
        print("Case failed");
        await this.AsyncLoader("LoseScene");
    }

    public async UniTask ConsumeAttempt(string solveText)
    {
        _leftAttempts--;
        if (_leftAttempts > 0) await m_view.ShowError(solveText);
        else await FailCase();
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
