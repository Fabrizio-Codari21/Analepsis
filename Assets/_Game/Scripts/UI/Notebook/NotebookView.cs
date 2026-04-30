using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotebookView : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera m_renderCamera;
    
    [Header("Notebook Tiltle")]
    [SerializeField] private TMP_Text m_titleText;
    [Header("Clue Button")]
    [SerializeField] private Transform m_buttonRoot;
    [SerializeField] private ButtonSetting m_buttonSetting;
    
    [Header("Detail Setting")]
    [SerializeField] private Transform m_detailRoot;
    [SerializeField,InfoBox("Los botones que este en los detalles")] private ButtonSetting m_detailButtonSetting;
    [SerializeField,InfoBox("Texto Description de clue")] private DynamicTextSetting  m_dynamicTextSetting;
    [SerializeField,InfoBox("Si Clue necesita un imagen")] private ImageSetting  m_imageSetting;
    
    [Header("Base UI")]
    [SerializeField] private ScrollRect m_scrollRect;
    [SerializeField] private Button m_next;
    [InfoBox("El width maxima que puede tener el texto")][Range(200f,700f)][SerializeField] private float m_textWidth = 600f;
    [SerializeField] private Button m_previous;
    
    
    private IActivity _activity;
    private void Start()
    { 
        _activity = GetComponentInParent<IActivity>();
        _activity.OnResume += () => { m_renderCamera.enabled = true; };
        _activity.OnPause += () => { m_renderCamera.enabled = false; };
        gameObject.SetActive(false);
    }
    

    public void SetTitle(string title)
    {
        m_titleText.text = title;
    }
    
    public void NextButtonAdd(UnityAction action) => ButtonAddListener(m_next, action);
    public void PreviousButtonAdd(UnityAction action) => ButtonAddListener(m_previous, action);
    public void RemoveNext() => ButtonRemoveListener(m_next);
    public void RemovePrevious() => ButtonRemoveListener(m_previous);
    
    private void ButtonAddListener(Button button,UnityAction action)
    {
        button.onClick.AddListener(action);
    }
    
    private void ButtonRemoveListener(Button button) => button.onClick.RemoveAllListeners();
    
    public ButtonFactoryObject CreateButton(string text)
    {
        return CreateButtonInternal(text, m_buttonRoot, m_buttonSetting);
    }

    public ButtonFactoryObject CreateDetailButton(string text)
    {
        return CreateButtonInternal(text, m_detailRoot, m_detailButtonSetting);
    }


    public ButtonFactoryObject CreateToggleButton(string text,Action doAction, Action undoAction,bool toggle = false)
    {
        var button = FlyweightFactory.Instance.Spawn<ToggleButtons>(m_buttonSetting,Vector3.zero,Quaternion.identity,m_buttonRoot);
        button.SetText(text);
        button.AddToggleButton(doAction, undoAction,toggle);
        return button;
    }

    
    
    private ButtonFactoryObject CreateButtonInternal(string text, Transform parent, ButtonSetting setting)  
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            setting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );
        button.SetText(text);
        button.SetInteractable(true);
        return button;
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
            obj.Despawn();
        }
        Despawn(m_buttonRoot);
    }
    public async UniTask PlayText(List<string> text, CancellationToken token) 
    {
        token.ThrowIfCancellationRequested();

        foreach (var item in text)
        {
            if(item == null) return;
            var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
            m_dynamicTextSetting,
            Vector3.zero,
            Quaternion.identity,
            m_detailRoot);

            t.SetText(item, m_dynamicTextSetting.size, m_dynamicTextSetting.color, m_textWidth, true);
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
    
    private void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }
}