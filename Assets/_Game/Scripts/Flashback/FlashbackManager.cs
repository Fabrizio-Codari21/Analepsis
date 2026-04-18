using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class FlashbackManager : PersistentSingleton<FlashbackManager>, IActivity
{
    [SerializeField] CCInputReader inputReader;
    [SerializeField] BoolEventChannel enableFlashback;
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
    ItemReference _currentItemInspected;
    //List<IInteractable> _allInteractables;

    
    public bool IsFlashbackOn() => _isFlashbackOn;
    //public void AddInteractable(GameObject interactable) => _allInteractables.Add(interactable);
    public void SetCurrentItem(ItemReference item) => _currentItemInspected = item;
    public Item CurrentItem => _currentItemInspected?.GetInspectItem();

    public event Action OnResume;
    public event Action OnPause;
    public event Action OnStop;

    void Start()
    {
        transitionPanel.gameObject.SetActive(false);
        transitionPanel.color -= new Color(0, 0, 0, transitionPanel.color.a);
    }

    public void SeeFlashback()
    {
        if (!_isFlashbackOn) _ = StartFlashback(_currentItemInspected); else _ = EndFlashback(_currentItemInspected);
    }

    Interactable _flashbackObject;
    public IInteractable GetFlashbackObject() => _flashbackObject;

    public async UniTask StartFlashback(ItemReference inspected = default)
    {
        bool transitionPanelActive = false;
        //Pause();
        //pushEvent.Raise(this);
        _isFlashbackOn = true;
        SetCurrentItem(inspected);
        transitionPanel.gameObject.SetActive(true);
        while (mainFlashbackShader.GetFloat("_Control") < 1f)
        {
            mainFlashbackShader.SetFloat("_Control", mainFlashbackShader.GetFloat("_Control") + 0.01f * transitionSpeed/5);
            //highlightShader.SetFloat("_Control", highlightShader.GetFloat("_Control") + 0.01f * transitionSpeed);

            if (!transitionPanelActive)
            {
                transitionPanel.color += new Color(0,0,0, 0.04f * transitionSpeed/5);
                if(transitionPanel.color.a >= 1f) transitionPanelActive = true;
            }
            else
            {
                if (!_flashbackObject)
                {
                    if (CurrentItem == null) print("null");
                    enableFlashback.Raise(false);
                    var transf = Instantiate(CurrentItem.flashbackInfo.flashbackTransform.gameObject);
                    _flashbackObject = Instantiate(CurrentItem.flashbackInfo.characterPrefab, transf.transform);
                    inspected.gameObject.SetActive(false);
                    _flashbackObject.OnFocus += SpawnName;
                    _flashbackObject.OnUnfocus += DespawnName;
                    //_flashbackObject.AddTip(new(CurrentItem.flashbackClue, TipOrder.Name));
                    flashbackInputReader.SetEnable(true);
                }
                //_flashbackObject.AddTip(new("Click on them or press 'F' to leave the flashback", TipOrder.ActionCost));
                transitionPanel.color += new Color(0, 0, 0, a: -0.02f * transitionSpeed/5);
            }

            await UniTask.Delay(20);
        }

        mainFlashbackShader.SetFloat("_Control", 1f);
        //highlightShader.SetFloat("_Control", 1f);
        transitionPanel.color -= new Color(0,0,0,transitionPanel.color.a);
        transitionPanel.gameObject.SetActive(false);
        flashbackInputReader.ExitFlashback += SeeFlashback; 
    }

    public async UniTask EndFlashback(ItemReference inspected = default)
    {
        bool transitionPanelActive = false;
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
                    _isFlashbackOn = false;
                    enableFlashback.Raise(true);
                    DespawnName();
                    _flashbackObject.OnFocus -= SpawnName;
                    _flashbackObject.OnUnfocus -= DespawnName;
                    if (CurrentItem.flashbackClue == null) CurrentItem.flashbackClue = CurrentItem.flashbackInfo.info;
                    SetCurrentItem(null);
                    _flashbackObject.ClearTip();
                    Destroy(_flashbackObject.gameObject);
                    inspected.gameObject.SetActive(true);
                    inputReader.SetEnable(true);
                }

                //if (!inspected.gameObject.activeInHierarchy) inspected.gameObject.SetActive(true);
                transitionPanel.color += new Color(0, 0, 0, a: -0.02f * transitionSpeed/5);
            }

            await UniTask.Delay(20);
        }

        mainFlashbackShader.SetFloat("_Control", 0f);
        //highlightShader.SetFloat("_Control", 0f);
        transitionPanel.color -= new Color(0, 0, 0, transitionPanel.color.a);
        flashbackInputReader.ExitFlashback -= SeeFlashback;
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
                                new Vector3(0,1.5f,0) + CurrentItem.flashbackInfo.flashbackTransform.position, 
                                Quaternion.identity);

        flashbackClueDisplay.SetText(CurrentItem.flashbackInfo.info, 1, Color.cyan);
        _ = flashbackClueDisplay.PlayTypeWriterEffect();
    }

    private void DespawnName()
    {
        if (flashbackClueDisplay) FlyweightFactory.Instance.Return(flashbackClueDisplay);
        flashbackClueDisplay = null;
    }

    //public void ToggleByFlashback(IFocus toggled)
    public void ToggleByFlashback(GameObject obj)
    {
        //foreach (var obj in _allInteractables)
        //print("llamado");
        //var obj = toggled as MonoBehaviour;
        if(_flashbackObject != null && obj != _flashbackObject)
        {
            //toggled.Unfocus();
            if (IsFlashbackOn() && obj.activeInHierarchy) obj.SetActive(false);
            else if (!IsFlashbackOn() && !obj.activeInHierarchy) obj.SetActive(true);
            print("Turning " + (obj.activeInHierarchy ? "on" : "off"));
        }
    }

    public void Resume()
    {
        flashbackInputReader.SetEnable(false);
        //popEvent.OnEventRaised -= SeeFlashback;
        OnResume?.Invoke();
        print("resume");
    }

    public void Pause()
    {
        print("pause");
        flashbackInputReader.SetEnable(false);
        inputReader.SetEnable(false);
        OnPause?.Invoke();
        //inspectionInputReader.SeeFlashback -= Exit;
        //inspectionInputReader.SetEnable(false);
        //popEvent.OnEventRaised += SeeFlashback;
    }

    public void Stop()
    {
        print("stop");
        OnStop?.Invoke();
        Pause();
    }

    public void Exit() => popEvent.Raise();

    public bool CanPopWithKey()
    {
        return true;
    }
}
