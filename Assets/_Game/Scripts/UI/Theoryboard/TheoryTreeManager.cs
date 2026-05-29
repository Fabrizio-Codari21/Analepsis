using Cysharp.Threading.Tasks;
using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

// esto despues se unifica con el theoryboard original
public class TheoryTreeManager : MonoBehaviour, IActivity
{
    [SerializeField] TheoryboardView view;
    
    [Space(15), Header("World Space Movement & Zoom")]
    public float scrollSpeed = 25f;
    public float zoomSpeed = 5f;
    [SerializeField] Transform treeAnchor;
    [SerializeField] RectTransform treeParent;
    [SerializeField] ScrollRect treeScroll;
    [SerializeField] private Canvas boardView;
    [SerializeField] private UIHoverDetector scrollHoverDetector;
    [SerializeField] private Vector3 _scrollScale = Vector3.one;
    [SerializeField] private Vector3 _scrollOffset = Vector3.zero;

    [Space(15), Header("Attempts")]
    [SerializeField] int maxSolveAttempts;
    public int attemptsLeft { get; private set; }
    [SerializeField] private bool isTest;

    #region Event

    [Space(15), Header("Event")]
    [SerializeField] private EventChannel m_openTheoryBoardChannel;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private BoardInputReader inputReaderBoard;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;

    #endregion


    //[SerializeField] CinemachinePanTilt camData;
    //[SerializeField] CinemachineCamera _camera;

    [Space(25), Header("SELECT A CASE TO PLAY")]
    public CaseResolution currentCase;

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
        OnPause?.Invoke();
        inputReaderBoard.SetEnable(false);
        enableCursor.Raise(false);
        view.gameObject.SetActive(false);
        //_camera.enabled = false;
    }

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderBoard.SetEnable();
        enableCursor.Raise(true);
        view.gameObject.SetActive(true);
        //_camera.enabled = true;
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
        //m_openTheoryBoardChannel.OnEventRaised += Open;
        //m_openTheoryBoardChannel.OnEventRaised += view.LoadMarkedClues;
        //inputReaderBoard.Close += Close;
        attemptsLeft = maxSolveAttempts;
        view.gameObject.SetActive(false);
        _open = false;
        //_camera.transform.localPosition += new Vector3(-UIManager.Instance.AspectRatioOffset(0.2f), UIManager.Instance.AspectRatioOffset(1), 0);

    }

    // placeholder, obvio
    bool _open = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!_open)
            {
                Open();
                view.LoadMarkedClues();
                _open = true;
            }
            else
            {
                Close();
                _open = false;
            }
        }
        MoveTreeScroll();
    }

    private void OnDestroy()
    {
        //m_openTheoryBoardChannel.OnEventRaised -= Open;
        //m_openTheoryBoardChannel.OnEventRaised -= view.LoadMarkedClues;
        //inputReaderBoard.Close -= Close;
    }

    #endregion

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

    private void ResetScrollAndScale()
    {

        if (treeScroll != null)
        {
            treeScroll.verticalNormalizedPosition = 0.5f;
            treeScroll.horizontalNormalizedPosition = 0.5f;
        }
        treeAnchor.localPosition = Vector3.zero;


        treeAnchor.localScale = _scrollScale;
    }


    public void MoveTreeScroll()
    {
        if (!treeAnchor.gameObject.activeInHierarchy) return;


        float moveStep = scrollSpeed * Time.deltaTime;
        Vector3 localMove = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) localMove -= new Vector3(0, moveStep, 0);
        else if (Input.GetKey(KeyCode.S)) localMove += new Vector3(0, moveStep, 0);
        if (Input.GetKey(KeyCode.D)) localMove -= new Vector3(moveStep, 0, 0);
        else if (Input.GetKey(KeyCode.A)) localMove += new Vector3(moveStep, 0, 0);

        treeAnchor.localPosition += localMove;


        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f && scrollHoverDetector != null && scrollHoverDetector.IsMouseHovering)
        {

            Vector3 mouseWorldPosBefore = GetMouseWorldPosOnCanvas();
            Vector3 mouseLocalPosBefore = treeAnchor.InverseTransformPoint(mouseWorldPosBefore);

            float zoomStep = Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
            Vector3 targetScale = treeAnchor.localScale + new Vector3(zoomStep, zoomStep, 0);

            targetScale.x = Mathf.Clamp(targetScale.x, _scrollScale.x / 2f, _scrollScale.x * 3f);
            targetScale.y = Mathf.Clamp(targetScale.y, _scrollScale.y / 2f, _scrollScale.y * 3f);

            treeAnchor.localScale = targetScale;

            Vector3 mouseWorldPosAfter = treeAnchor.TransformPoint(mouseLocalPosBefore);


            Vector3 worldOffset = mouseWorldPosBefore - mouseWorldPosAfter;
            Vector3 localOffset = treeAnchor.parent.InverseTransformVector(worldOffset);

            treeAnchor.localPosition += localOffset;
        }

        if (!Input.GetKeyDown(KeyCode.F)) return;
        ResetScrollAndScale();
    }

    private Vector3 GetMouseWorldPosOnCanvas()
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(treeParent, Input.mousePosition, Camera.main, out Vector3 worldPoint))
        {
            return worldPoint;
        }

        return treeParent.position;
    }
}
