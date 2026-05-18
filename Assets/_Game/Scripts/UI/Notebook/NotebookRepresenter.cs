using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using PrimeTween;

public class NotebookRepresenter : MonoBehaviour,ITakeable
{
    
    
    [Header("Animation")]
    [SerializeField] private Animator m_animator;
    
    
    [Header("Left")]
    [SerializeField] private TMP_Text m_titleText;
    [SerializeField] private Button m_next;
    [SerializeField] private Button m_previous;
    [SerializeField] private Transform m_buttonRoot;
    
    
    [Header("Right")]
    [SerializeField] private Transform m_detailRoot;
    [SerializeField] private ScrollRect m_scrollRect;
    
    
    [Header("UI Setting")]
    [SerializeField] private ButtonSetting m_buttonSetting;
    [SerializeField] private ButtonSetting m_detailButtonSetting;
    [SerializeField] private DynamicTextSetting  m_dynamicTextSetting;
    [SerializeField] private ImageSetting  m_imageSetting;
    [Range(10f,1000f)][SerializeField] private float m_textWidth = 600f;

    [Header("Pages")]
    [SerializeField] private Canvas[] m_notebookNormalPages;
    [SerializeField] private Canvas[] m_treeCanva;
    
    [Header("Transition")]
    [SerializeField] private Vector3 m_rotateValue;
    [SerializeField] private float m_transitionSpeed;
    [SerializeField] private Vector3 m_positionValue;
    private Vector3 _initialPosition;
    private Vector3 _initialRotation;
    
    
    [SerializeField] private DialogueTree m_dialogueTree;
    private void Start()
    {

        _initialPosition = transform.localPosition;
        _initialRotation = transform.localEulerAngles ;
        gameObject.SetActive(false);
    }

    #region Internal

    private void ButtonAddListener(Button button,UnityAction action)
    {
        button.onClick.AddListener(action);
    }
    
    private void ButtonRemoveListener(Button button) => button.onClick.RemoveAllListeners();
    private ButtonFactoryObject CreateButtonInternal(string text, Transform parent, ButtonSetting setting, bool updateScroll = false)  
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            setting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );

        button.SetText(text);
        button.SetInteractable(true);
        if (updateScroll) button.UpdateScroll(); //idealmente que en ese metodo se resetee el scroll
        button.MoveToLast();
        m_scrollRect.verticalNormalizedPosition = 0;
        return button;
    }
    
    
    #endregion

    #region External
    public void NextButtonAdd(UnityAction action) => ButtonAddListener(m_next, action);
    public void PreviousButtonAdd(UnityAction action) => ButtonAddListener(m_previous, action);
    public void RemoveNext() => ButtonRemoveListener(m_next);
    public void RemovePrevious() => ButtonRemoveListener(m_previous);
    public void SetTitle(string title)
    {
        m_titleText.text = title;
    }
    
    public ButtonFactoryObject CreateButton(string text)
    {
        return CreateButtonInternal(text, m_buttonRoot, m_buttonSetting);
    }

    public ButtonFactoryObject CreateDetailButton(string text)
    {
        return CreateButtonInternal(text, m_detailRoot, m_detailButtonSetting, true);
    }

    public ButtonFactoryObject CreateCustomButton(string text, Transform parent, ButtonSetting setting)
    {
        return CreateButtonInternal(text, parent, setting, true);
    }

 
    public void ClearDetail()  
    {
        Despawn(m_detailRoot);
    }

    public void ClearButton()  
    {
        var buttons = m_buttonRoot.GetComponentsInChildren<ButtonFactoryObject>();
        foreach (ButtonFactoryObject obj in buttons)
        {
            obj.ClearChildren();
            obj.Despawn();
        }
        Despawn(m_buttonRoot);
    }
    public async UniTask PlayText(List<string> text, CancellationToken token, Transform parent = default, float sizeOverride = default) 
    {
        token.ThrowIfCancellationRequested();

        foreach (var item in text)
        {
            if(item == null) return;
            var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dynamicTextSetting,
            Vector3.zero,
            Quaternion.identity,
            parent != null ? parent : m_detailRoot);

            t.SetText(
                item, 
                !Mathf.Approximately(sizeOverride, 0) ? sizeOverride : m_dynamicTextSetting.size,
                m_dynamicTextSetting.color, 
                m_textWidth, 
                true, 
                item == text.Last() ? 1 : 0);

            t.ToLast();
            await UniTask.NextFrame(token);
            token.ThrowIfCancellationRequested();
            m_scrollRect.verticalNormalizedPosition = 0;
            await t.PlayTypeWriterEffect(externalToken: token);
        }

    }
    
    public void CreateImage(Sprite sprite)
    {
        var image = FlyweightFactory.Instance.Spawn<UIImage>(m_imageSetting, Vector3.zero, Quaternion.identity, m_detailRoot);
        image.SetImage(sprite);
        
    }
    
    public void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }
    
    #endregion

    
    private  Sequence _sequence = new();
    
  
    
    private async UniTask RotateNotebook()
    {
        _sequence.Stop();
        _sequence = Sequence.Create();

    
        _ = _sequence.Group(Tween.LocalPosition(transform, m_positionValue, m_transitionSpeed, Ease.OutCirc));
        
   
        Quaternion targetQuat = Quaternion.Euler(m_rotateValue);
        _ = _sequence.Group(Tween.LocalRotation(
            target: transform,
            endValue: targetQuat,
            duration: m_transitionSpeed,
            ease: Ease.Linear
        ));

        await _sequence;
    }


    public async UniTask ToggleTree(bool toggle,NpcIdentity key )
    {
        await RotateNotebook();
        ToggleCanvasPages(toggle);
        await m_dialogueTree.ToggleTree(toggle,key);
        
    }

     private void ToggleCanvasPages(bool showTree)
    {
        foreach (var canvas in m_notebookNormalPages) if (canvas != null) canvas.gameObject.SetActive(!showTree);
        
        
        foreach (var canvas in m_treeCanva) if (canvas != null) canvas.gameObject.SetActive(showTree);
        
    }
 
    public async UniTask ResetNotebookAnimation()
    {
        _sequence.Stop();
        _sequence = Sequence.Create();
        
      
        _ = _sequence.Group(Tween.LocalPosition(transform, _initialPosition, m_transitionSpeed, Ease.OutCirc));
        
   
        Quaternion initialQuat = Quaternion.Euler(_initialRotation);
        _ = _sequence.Group(Tween.LocalRotation(
            target: transform,
            endValue: initialQuat,
            duration: m_transitionSpeed,
            ease: Ease.Linear
        ));

        await _sequence;
        
        
        ToggleCanvasPages(false);
        
    }
    public void TryTake(Transform takeRoot)
    {
        transform.localPosition = _initialPosition;
        transform.localEulerAngles = _initialRotation;
        transform.SetParent(takeRoot,false);
        gameObject.SetActive(true);
        m_animator.CrossFade("Hand_Open",0f);

        ToggleCanvasPages(false);
    }
    public void Release()
    {
        transform.localPosition = _initialPosition;
        transform.localEulerAngles = _initialRotation;
        transform.SetParent(null, false);
        _ = m_dialogueTree.ToggleTree(false);
        gameObject.SetActive(false);

    }
}