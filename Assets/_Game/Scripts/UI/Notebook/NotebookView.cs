using System.Threading;
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
    
    [SerializeField] private ButtonSetting m_buttonSetting;
    [SerializeField] private DynamicTextSetting  m_dynamicTextSetting;
    [SerializeField] private ScrollRect m_scrollRect;
    [SerializeField] private Button m_next;
    [SerializeField] private Button m_previous;
    private void Start()
    {
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
    public async UniTask PlayText(string text, CancellationToken token) 
    {
        token.ThrowIfCancellationRequested();
        var t = FlyweightFactory.Instance.Spawn<DynamicText>(
            m_dynamicTextSetting,
            Vector3.zero,
            Quaternion.identity,
            m_detailRoot
        );
        t.SetText(text,m_dynamicTextSetting.size,m_dynamicTextSetting.color);
        await UniTask.NextFrame(token);
        token.ThrowIfCancellationRequested();
        m_scrollRect.verticalNormalizedPosition = 0;
        await t.PlayTypeWriterEffect(externalToken: token);
    }
    
    private void Despawn(Transform root) 
    {
        foreach (var f in root.GetComponentsInChildren<IFlyweight>())
        {
            FlyweightFactory.Instance.Return(f);
        }
    }
}