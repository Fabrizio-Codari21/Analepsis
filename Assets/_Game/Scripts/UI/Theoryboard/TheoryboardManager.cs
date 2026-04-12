using Sirenix.OdinInspector;
//using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TheoryboardManager : MonoBehaviour, IActivity
{
    [SerializeField] TheoryboardView view;

    [SerializeField] private CCInputReader inputReader;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private BoardInputReader inputReaderBoard;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
    [SerializeField] private TransformEventChannel m_cameraRotationEventChannel;

    //[SerializeField] NotebookManager notebookManager;
    [SerializeField] private Canvas boardView;

    [SerializeField] Transform playerMenuTransform;
    [SerializeField] Transform boardTransform;
    [SerializeField] GameObject player;
    Tuple<Vector3, Quaternion> _playerTransform;
    //Transform _oldLookAt;

    [Space(20), Header("WHAT'S THE RIGHT ANSWER?")]
    [ShowInInspector, TableList, DictionaryDrawerSettings(KeyLabel = "Role", ValueLabel = "Proof")] 
    public Dictionary<Whodunnit, IClue> correctAnswer;

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
        //playerCamera.Camera.LookAt = _oldLookAt;
        print("llamado");
        player.transform.position = _playerTransform.Item1;
        player.transform.rotation = _playerTransform.Item2;
        m_cameraRotationEventChannel.Raise(player.transform);
        
    }

    public void Resume()
    {
        OnResume?.Invoke();
        inputReaderBoard.SetEnable();
        enableCursor.Raise(true);
        _playerTransform = new Tuple<Vector3, Quaternion>(player.transform.position, player.transform.rotation);
        //_oldLookAt = playerCamera.Camera.LookAt;
        //playerCamera.Camera.LookAt = boardTransform;
        player.transform.position = playerMenuTransform.position;
        player.transform.rotation = playerMenuTransform.rotation;
        m_cameraRotationEventChannel.Raise(playerMenuTransform);
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
        inputReader.OpenTheoryBoard += Open;
        inputReader.OpenTheoryBoard += view.LoadMarkedClues;
        inputReaderBoard.Close += Close;
    }

    void Open() => pushEvent.Raise(this);
    void Close() => popEvent.Raise();

    public void SolveCase()
    {
        print("Case solved");
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
}
