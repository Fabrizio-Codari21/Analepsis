using Cysharp.Threading.Tasks;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FlashbackManager : PersistentSingleton<FlashbackManager>, IActivity
{
    [SerializeField] CCInputReader inputReader;
    [SerializeField] FlashbackInputReader flashbackInputReader;
    [SerializeField] InspectionInputReader inspectionInputReader;
    [SerializeField] Material mainFlashbackShader;
    [SerializeField] Material highlightShader;
    [SerializeField] Image transitionPanel;
    [SerializeField] float transitionSpeed;
    [SerializeField] private IActivityEvent pushEvent;
    [SerializeField] private EventChannel popEvent;
    [SerializeField] DynamicTextSetting displaySetting;

    bool _isFlashbackOn = false;
    Item _currentItemInspected;

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    void Start()
    {
        transitionPanel.gameObject.SetActive(false);
        transitionPanel.color -= new Color(0, 0, 0, transitionPanel.color.a);
    }

    public void SetCurrentItem(Item item) => _currentItemInspected = item;
    public void SeeFlashback()
    {
        if (!_isFlashbackOn) StartFlashback(_currentItemInspected); else EndFlashback(_currentItemInspected);
    }

    Interactable _flashbackObject;
    public async UniTask StartFlashback(Item inspected = default)
    {
        bool transitionPanelActive = false;
        //inputReader.SetEnable(false);
        SetCurrentItem(inspected);
        transitionPanel.gameObject.SetActive(true);
        while (mainFlashbackShader.GetFloat("_Control") < 1f)
        {
            mainFlashbackShader.SetFloat("_Control", mainFlashbackShader.GetFloat("_Control") + 0.01f * transitionSpeed/5);
            //highlightShader.SetFloat("_Control", highlightShader.GetFloat("_Control") + 0.01f * transitionSpeed);

            if (!transitionPanelActive)
            {
                transitionPanel.color += new Color(0,0,0,a: 0.04f * transitionSpeed/5);
                if(transitionPanel.color.a >= 1f) transitionPanelActive = true;
            }
            else
            {
                if (!_flashbackObject)
                {
                    _flashbackObject = Instantiate(inspected.flashbackInfo.characterPrefab, inspected.flashbackInfo.flashbackTransform);
                    _flashbackObject.OnFocus += SpawnName;
                    _flashbackObject.OnUnfocus += DespawnName;
                }
                _flashbackObject.AddTip(new(inspected.flashbackClue, TipOrder.Name));
                transitionPanel.color += new Color(0, 0, 0, a: -0.02f * transitionSpeed/5);
            }

            await UniTask.Delay(20);
        }

        mainFlashbackShader.SetFloat("_Control", 1f);
        //highlightShader.SetFloat("_Control", 1f);
        transitionPanel.color -= new Color(0,0,0,transitionPanel.color.a);
        transitionPanel.gameObject.SetActive(false);
        flashbackInputReader.ExitFlashback += SeeFlashback; 
        flashbackInputReader.SetEnable(true);
        _isFlashbackOn = true;
    }

    public async UniTask EndFlashback(Item inspected = default)
    {
        bool transitionPanelActive = false;

        flashbackInputReader.SetEnable(false);
        transitionPanel.gameObject.SetActive(true);
        while (mainFlashbackShader.GetFloat("_Control") > 0f)
        {
            mainFlashbackShader.SetFloat("_Control", mainFlashbackShader.GetFloat("_Control") - 0.01f * transitionSpeed/5);
            //highlightShader.SetFloat("_Control", highlightShader.GetFloat("_Control") - 0.01f * transitionSpeed);

            if (!transitionPanelActive)
            {
                transitionPanel.color += new Color(0, 0, 0, a: 0.04f * transitionSpeed/5);
                if (transitionPanel.color.a >= 1f) transitionPanelActive = true;
            }
            else
            {
                if (_flashbackObject)
                {
                    DespawnName();
                    _flashbackObject.OnFocus -= SpawnName;
                    _flashbackObject.OnUnfocus -= DespawnName;
                    SetCurrentItem(null);
                    Destroy(_flashbackObject.gameObject);
                }
                if (inspected.flashbackClue == null) inspected.flashbackClue = inspected.flashbackInfo.info;
                //if (!inspected.gameObject.activeInHierarchy) inspected.gameObject.SetActive(true);
                transitionPanel.color += new Color(0, 0, 0, a: -0.02f * transitionSpeed/5);
            }

            await UniTask.Delay(20);
        }

        mainFlashbackShader.SetFloat("_Control", 0f);
        //highlightShader.SetFloat("_Control", 0f);
        transitionPanel.color -= new Color(0, 0, 0, transitionPanel.color.a);
        //inputReader.SetEnable(true);
        flashbackInputReader.ExitFlashback -= SeeFlashback;
        _isFlashbackOn = false;
    }

    DynamicText flashbackClueDisplay;
    private void SpawnName()
    {
        if (_currentItemInspected == null)
        {
            Debug.Log("No item selected");
            return;
        }
        
        flashbackClueDisplay = FlyweightFactory.Instance.Spawn<DynamicText>(
                                displaySetting, 
                                new Vector3(0,1.5f,0) + _currentItemInspected.flashbackInfo.flashbackTransform.position, 
                                Quaternion.identity,
                                _currentItemInspected.flashbackInfo.flashbackTransform);

        flashbackClueDisplay.SetText(_currentItemInspected.flashbackInfo.info, 1, Color.cyan);
        _ = flashbackClueDisplay.PlayTypeWriterEffect();
    }

    private void DespawnName()
    {
        if (flashbackClueDisplay) FlyweightFactory.Instance.Return(flashbackClueDisplay);
        flashbackClueDisplay = null;
    }

    public void Resume()
    {
        OnResume?.Invoke();
    }

    public void Pause()
    {
        OnPause?.Invoke();
        flashbackInputReader.SetEnable(false);
        inputReader.SetEnable(true);
        //popEvent.Raise();
        //popEvent.OnEventRaised -= SeeFlashback;
    }

    public void Stop()
    {
        OnStop?.Invoke();
        Pause();
    }

    public bool CanPopWithKey()
    {
        return true;
    }
}
