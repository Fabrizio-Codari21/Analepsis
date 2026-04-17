using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotebookView : MonoBehaviour
{
    [SerializeField] private TMP_Text m_titleText;
    [SerializeField] private Transform m_buttonRoot;
    [SerializeField] private Transform m_detailRoot;
    [SerializeField] private Camera m_renderCamera;
    [SerializeField]private ButtonSetting m_buttonSetting;
    [SerializeField] private DynamicTextSetting  m_dynamicTextSetting;
    [SerializeField] private ImageSetting  m_imageSetting;
    [SerializeField] private ScrollRect m_scrollRect;
    [SerializeField] private Button m_next;
    [SerializeField] private Button m_previous;

    private IActivity _activity;
    [Range(200f,500f)][SerializeField] private float m_textWidth = 200f;
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
        return CreateButtonInternal(text, m_buttonRoot);
    }

    public ButtonFactoryObject CreateDetailButton(string text)
    {
        return CreateButtonInternal(text, m_detailRoot);
    }
    
    private ButtonFactoryObject CreateButtonInternal(string text, Transform parent)  
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            m_buttonSetting,
            Vector3.zero,
            Quaternion.identity,
            parent
        );

        button.SetText(text);
        button.SetInteractable(true);
        button.MoveToLast();
        return button;
    }

    public void ClearDetail()  
    {
        Despawn(m_detailRoot);
    }

    public void ClearButton()  
    {
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

            t.SetText(item, m_dynamicTextSetting.size, m_dynamicTextSetting.color, m_textWidth);
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