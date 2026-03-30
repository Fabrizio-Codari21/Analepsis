using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class NotebookManager : SerializedMonoBehaviour, IActivity
{
    public static NotebookManager instance;
    public CCInputReader inputReader;
    public BoolEventChannel enableCursor;
    [SerializeField] private NoteBookInputReader inputReaderNoteBook;
    [SerializeField] private IActivityEvent activityEvent;
    [SerializeField] private EventChannel popEvent;
    private bool isEnable;
    public enum NotebookPage
    {
        Default,
        Log,
        Objects,
    }

    private void Awake() 
    {
        if (!instance) instance = this; else Destroy(gameObject); 
    }

    #region Notebook Controls
    [Header("NOTEBOOK")]
    public GameObject notebookUI;
    public Button previousPageButton;
    public Button nextPageButton;
    public TextMeshProUGUI menuName;
    [SerializeField] Dictionary<NotebookPage, NotebookMenu> _notebookPages = new();
    NotebookPage _lastPageOpen;

    public void OpenPage(NotebookPage page = NotebookPage.Default)
    {
        if (page == NotebookPage.Default && _lastPageOpen != default) page = _lastPageOpen;
        if (!notebookUI.activeInHierarchy) notebookUI.SetActive(true);
      
        foreach (KeyValuePair<NotebookPage, NotebookMenu> item in _notebookPages)
        {
            if (item.Value.gameObject.activeInHierarchy) item.Value.gameObject.SetActive(false);
        }

        _notebookPages[page].gameObject.SetActive(true);
        _lastPageOpen = page;
        menuName.text = page.ToString();

        foreach (Transform child in logTextContainer) Destroy(child.gameObject);
    }

    public void NextPage(bool nextOrPrevious = true)
    {
        if (nextOrPrevious && _lastPageOpen != NotebookPage.Objects) OpenPage(_lastPageOpen + 1);
        else if(!nextOrPrevious && _lastPageOpen != NotebookPage.Log) OpenPage(_lastPageOpen - 1);

        menuName.text = _lastPageOpen.ToString();
    }

    public void CloseNotebook() 
    {
        notebookUI.SetActive(false);
    
    }

    #endregion

    #region Log Menu
    [Header("LOG")]
    public Transform logListContainer;
    public Transform logTextContainer;
    public Button logButton;
    public TextMeshProUGUI logText;
    public Color logColor;
    public float logTextSpeed;

    [OdinSerialize] Dictionary<string, string> _dialogueLogs = new();

    public void SaveLogToNotebook(string dialogueID, string log) 
    {
        _dialogueLogs.Add(dialogueID, log);
        Button button = Instantiate(logButton, logListContainer);
        button.GetComponentInChildren<TextMeshProUGUI>().text = dialogueID;
        button.onClick.AddListener(() => PlayLog(dialogueID, logTextSpeed));
    }

    public void PlayLog(string dialogueID, float speed = 10)
    {
        CloseLog(logTextContainer);
        TextMeshProUGUI log = Instantiate(logText, logTextContainer);
        //print(_dialogueLogs[dialogueID]);
        log.text = "";
        log.color = logColor;
        var logTime = DialogueManager.instance.BuildText(log, _dialogueLogs[dialogueID], speed);
        //log.text = _dialogueLogs[dialogueID];

        this.WaitAndThen(timeToWait: logTime, () =>
        {
            print(log.text);
            Button closeButton = Instantiate(logButton, logTextContainer);
            closeButton.GetComponentInChildren<TextMeshProUGUI>().text = "[Close Log]";
            closeButton.onClick.AddListener(() => CloseLog(logTextContainer));
        },
        cancelCondition: () => false);
    }

    public void CloseLog(Transform container)
    {
        foreach (Transform child in container) Destroy(child.gameObject);
    }

    #endregion

    #region Clue Menu

    [Header("OBJECTS")]
    public Transform clueListContainer;
    public Transform clueInfoContainer;
    public Button clueButton;
    public TextMeshProUGUI clueText;

    [OdinSerialize] private Dictionary<string, Clue> _clueRegistry = new();

    public void SaveClueToNotebook(string clueID, Clue clue)
    {
        _clueRegistry.TryAdd(clueID, clue);
        Button button = Instantiate(clueButton, clueListContainer);
        button.GetComponentInChildren<TextMeshProUGUI>().text = clueID;
        button.onClick.AddListener(() => ShowClues(clueID));
    }

    public void ShowClues(string clueID)
    {
        CloseLog(clueInfoContainer);
        var clueInfo = _clueRegistry[clueID];
        foreach (var clue in clueInfo.clues)
        {
            TextMeshProUGUI text = Instantiate(clueText, clueInfoContainer);
            text.text = $"- {clue}";
            text.color = logColor;
        }
        // instanciar render texture/imagen del objeto, a definir
        Button closeButton = Instantiate(logButton, clueInfoContainer);
        closeButton.GetComponentInChildren<TextMeshProUGUI>().text = "[Close Clues]";
        closeButton.onClick.AddListener(() => CloseLog(clueInfoContainer));
    }

    #endregion

    void Start()
    {
        CloseNotebook();
        previousPageButton.onClick.AddListener(() => NextPage(false));
        nextPageButton.onClick.AddListener(() => NextPage(true));

        inputReader.OpenNotebook += () => activityEvent.Raise(this);

        inputReaderNoteBook.Close += () => popEvent.Raise();
    }


    private void RequestOpen(bool enable)
    {
        if(enable == isEnable) return;

        if(enable) OpenPage(NotebookPage.Log);
        else CloseNotebook();
        isEnable = enable;


    }


    #region Interface
    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    public void Resume()
    {
        OnResume?.Invoke();

        RequestOpen(true);
        enableCursor.Raise(true);

        inputReaderNoteBook.SetEnable();
    }

    public void Pause()
    {
        OnPause?.Invoke();
        RequestOpen(false);
        enableCursor.Raise(false);
        inputReaderNoteBook.SetEnable(false);
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }
    #endregion
}
