using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaseSelector : MonoBehaviour, IActivity
{
    [Header("INSERT CASES")]
    public List<SelectableCaseInfo> allCasesAvailable;
    bool _orderByTime = false;

    [Space(10), Header("REFERENCES")]
    public SelectableCase casePrefab;
    public GameObject selectorMenu;
    public LayoutGroup caseLayout;
    public Button backButton;
    public Button orderByTimeButton;

    [Space(10), Header("EVENTS")]
    [SerializeField] private IActivityEvent m_pushActivity;
    [SerializeField] private EventChannel m_popActivity;
    [SerializeField] private BoolEventChannel m_cursorEnable;

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backButton.onClick.AddListener(m_popActivity.Raise);
        orderByTimeButton.onClick.AddListener(OrderByTime);

        _interact = GetComponent<IInteractable>();
        _interact.OnFocus += SpawnName;
        _interact.OnUnfocus += DespawnName;
        _interact.OnStart += DespawnName;
        _interact.OnStart += () => m_pushActivity.Raise(this);
        _tipProvider = GetComponent<ITipProvider>();
        _tipProvider.AddTip(new Tip($"Go back in time?", TipOrder.InteractionType));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OrderByTime()
    {
        _orderByTime = !_orderByTime;
        orderByTimeButton.GetComponentInChildren<TextMeshProUGUI>().text = _orderByTime ? "Logical" : "Chronological";
        OpenSelector();
    }

    public void OpenSelector()
    {
        var children = caseLayout.transform.GetChildren();
        foreach (var c in children) Destroy(c.gameObject);

        selectorMenu.SetActive(true);
        var cases = _orderByTime ? allCasesAvailable.OrderBy(x => x.year).ToList() : allCasesAvailable;
        foreach (var c in cases)
        {
            GameObject newCase = Instantiate(casePrefab.gameObject, caseLayout.transform);
            newCase.GetComponent<SelectableCase>().Assign(c, allCasesAvailable.IndexOf(c));
        }
    }
    
    public void CloseSelector()
    {
        var children = caseLayout.transform.GetChildren();
        foreach (var c in children) Destroy(c.gameObject);

        selectorMenu.SetActive(false);
    }

    [Space(10), Header("INTERACTION")]
    [SerializeField] private DynamicTextSetting m_nameTextSetting;
    [SerializeField] private Vector3 m_textPositionOffset;
    private IInteractable _interact;
    private ITipProvider _tipProvider;
    private DynamicText _text;

    private void SpawnName()
    {
        _text = FlyweightFactory.Instance.Spawn<DynamicText>(m_nameTextSetting, m_textPositionOffset + transform.position, Quaternion.identity, transform);
        _text.SetText("Select a Case", 2, m_nameTextSetting.color);
        _ = _text.PlayTypeWriterEffect();
    }

    private void DespawnName()
    {
        if (_text) FlyweightFactory.Instance.Return(_text);
        _text = null;
    }

    public void Resume()
    {
        OnResume?.Invoke();
        m_cursorEnable.Raise(true);
        OpenSelector();
    }

    public void Pause()
    {
        OnPause?.Invoke();
        m_cursorEnable.Raise(false);
        CloseSelector();
    }

    public void Stop()
    {
        OnStop?.Invoke();
        OnPause?.Invoke();
        m_cursorEnable.Raise(false);
        CloseSelector();
    }

    public bool CanPopWithKey()
    {
        return true;
    }
}


