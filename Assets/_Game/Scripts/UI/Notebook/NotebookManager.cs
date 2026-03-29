using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class NotebookManager : MonoBehaviour
{
    public static NotebookManager instance;

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
    public List<NotebookMenu> notebookMenues;
    Dictionary<NotebookPage, NotebookMenu> _notebookPages = new();
    NotebookPage _lastPageOpen;

    public void OpenPage(NotebookPage page = NotebookPage.Default)
    {
        if (page == NotebookPage.Default && _lastPageOpen != default) page = _lastPageOpen;
        if (!notebookUI.activeInHierarchy) notebookUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (KeyValuePair<NotebookPage, NotebookMenu> item in _notebookPages)
        {
            if (item.Value.gameObject.activeInHierarchy) item.Value.gameObject.SetActive(false);
        }

        _notebookPages[page].gameObject.SetActive(true);

        foreach (Transform child in logTextContainer) Destroy(child.gameObject);
    }

    public void NextPage(bool nextOrPrevious = true)
    {
        if (nextOrPrevious && _lastPageOpen != NotebookPage.Objects) OpenPage(_lastPageOpen + 1);
        else if(!nextOrPrevious && _lastPageOpen != NotebookPage.Log) OpenPage(_lastPageOpen - 1);
    }

    public void CloseNotebook() 
    {
        notebookUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    Dictionary<string, string> _dialogueLogs = new();

    public void SaveLogToNotebook(string dialogueID, string log) 
    {
        _dialogueLogs.Add(dialogueID, log);
        Button button = Instantiate(logButton, logListContainer);
        button.GetComponentInChildren<TextMeshProUGUI>().text = dialogueID;
        button.onClick.AddListener(() => PlayLog(dialogueID, logTextSpeed));
    }

    public void PlayLog(string dialogueID, float speed = 10)
    {
        CloseLog();
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
            closeButton.onClick.AddListener(() => CloseLog());
        },
        cancelCondition: () => false);
    }

    public void CloseLog()
    {
        foreach (Transform child in logTextContainer) Destroy(child.gameObject);
    }

    #endregion

    void Start()
    {
        foreach (NotebookMenu page in notebookMenues)
        {
            _notebookPages.Add(page.pageType, page);
        }
        CloseNotebook();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) 
        {
            if (!notebookUI.activeInHierarchy) OpenPage(NotebookPage.Log); else CloseNotebook();
        }
    }
}
