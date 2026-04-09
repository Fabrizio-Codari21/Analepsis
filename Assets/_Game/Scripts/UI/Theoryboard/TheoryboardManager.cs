using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using System;
using Unity.Cinemachine;
using UnityEngine;

public class TheoryboardManager : MonoBehaviour, IActivity
{
    [SerializeField] private CCInputReader inputReader;
    [SerializeField] private BoolEventChannel enableCursor;
    [SerializeField] private BoardInputReader inputReaderBoard;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;

    [SerializeField] NotebookManager notebookManager;
    [SerializeField] private Canvas boardView;
    [SerializeField] private SimpleCamera playerCamera;

    [SerializeField] Transform playerMenuTransform;
    [SerializeField] Transform boardTransform;
    [SerializeField] GameObject player;
    Tuple<Vector3, Quaternion> _playerTransform;
    Transform _oldLookAt;

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
        inputReader.SetEnable(true);
        enableCursor.Raise(false);

        //playerCamera.Camera.LookAt = _oldLookAt;
        print("llamado");
        player.transform.position = _playerTransform.Item1;
        player.transform.rotation = _playerTransform.Item2;
    }

    public void Resume()
    {
        OnResume?.Invoke();
        inputReader.SetEnable(false);
        enableCursor.Raise(true);

        _playerTransform = new Tuple<Vector3, Quaternion>(player.transform.position, player.transform.rotation);
        //_oldLookAt = playerCamera.Camera.LookAt;

        //playerCamera.Camera.LookAt = boardTransform;
        player.transform.position = playerMenuTransform.position;
        player.transform.rotation = playerMenuTransform.rotation;
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
    #endregion

    private void Start()
    {
        //boardView.renderMode = RenderMode.WorldSpace;
        //boardView.worldCamera = Camera.current;
        inputReader.OpenTheoryBoard += Open;
        inputReaderBoard.Close += Close;
    }

    void Open() => pushEvent.Raise(this);
    void Close() => popEvent.Raise();
}
