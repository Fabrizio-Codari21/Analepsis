using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;
using TMPro;

public class TheoryboardManager : MonoBehaviour, IActivity
{
    [SerializeField] TheoryboardView view;
    [SerializeField] int maxSolveAttempts;
    public int attemptsLeft { get; private set; }

    [SerializeField] private EventChannel m_openTheoryBoardChannel;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private BoardInputReader inputReaderBoard;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
    [SerializeField] private TransformEventChannel m_cameraRotationEventChannel;

    //[SerializeField] NotebookManager notebookManager;
    [SerializeField] private Canvas boardView;

  
    [SerializeField] CinemachinePanTilt camData;
    [SerializeField] CinemachineCamera _camera;
    
    //Transform _oldLookAt;

    [Space(25), Header("SELECT A CASE TO PLAY")]
    public CaseResolution currentCase;
    ////[DictionaryDrawerSettings(KeyLabel = "Role", ValueLabel = "Proof")] 
    //public SerializedDictionary<Whodunnit, IClue> correctAnswer = new();

    #region IActivity
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public bool CanPopWithKey()
    {
        return true;
    }

    public void Pause()
    {
        //var newTransform = Instantiate(new GameObject("View"), _playerTransform.Item1, _playerTransform.Item2).transform;

        OnPause?.Invoke();
        inputReaderBoard.SetEnable(false);
        enableCursor.Raise(false);
       
        _camera.enabled = false;
        
    }

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderBoard.SetEnable();
        enableCursor.Raise(true);
        _camera.enabled = true;
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }
    #endregion

    private void Start()
    {
        //boardView.renderMode = RenderMode.WorldSpace;
        //boardView.worldCamera = Camera.current;
        m_openTheoryBoardChannel.OnEventRaised += Open;
        m_openTheoryBoardChannel.OnEventRaised += view.LoadMarkedClues;
        inputReaderBoard.Close += Close;
        attemptsLeft = maxSolveAttempts;
        _camera.transform.localPosition += new Vector3(
            -UIManager.Instance.AspectRatioOffset(0.2f), 
            UIManager.Instance.AspectRatioOffset(1), 
            0);

    }

    void Open() => pushEvent.Raise(this);
    void Close() => popEvent.Raise();

    public async UniTask SolveCase(int answerID = 0, string answerName = "")
    {
        print(answerID == 0
            ? $"Solved with true answer: {answerName}" 
            : $"Solved with alternative answer: {answerName}");

        await this.AsyncLoader("WinScene");
        WinManager.Instance.SetConclusion(answerID);
    }
    public async UniTask FailCase()
    {
        print("Case failed");
        await this.AsyncLoader("LoseScene");
    }

    public async UniTask ConsumeAttempt(TextMeshProUGUI solveText)
    {
        attemptsLeft--;
        if (attemptsLeft > 0) await view.ShowError(solveText);
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
    //Place
}
