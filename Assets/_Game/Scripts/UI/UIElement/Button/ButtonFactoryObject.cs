using TMPro;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PrimeTween; 

public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField] protected Button m_button;
    [SerializeField] protected Image m_buttonImage;
    [SerializeField,CanBeNull] protected TextMeshProUGUI m_text;
    public override void Despawn()
    {
       base.Despawn();
       m_button.onClick.RemoveAllListeners();
    }

    public void SetFill(float fill)
    {
        if(m_buttonImage) m_buttonImage.fillAmount = fill; 
    }

    public void SetImageColor(Color color)
    {
        if(m_buttonImage) m_buttonImage.color = color;
    }
    public async UniTask PlayImageFill(float fill,float duration = 0.5f)
    {
        if (m_buttonImage == null) return;
        Tween.StopAll(m_buttonImage.gameObject);
        var seq = Sequence.Create();
        _ = seq.Group(Tween.UIFillAmount(m_buttonImage, fill, duration, Ease.OutQuint));
        await seq;
    }

    public void SetText(string text)
    {
        if(!m_text) return;
        m_text.text = text;
    }

    public void MoveToLast()
    {
        transform.SetAsLastSibling();
    }

    public void MoveToFirst() { transform.SetAsFirstSibling(); }
    public void MoveToPosition(int position) { transform.SetSiblingIndex(position); }
    public int GetPosition() { return transform.GetSiblingIndex(); }
    
    public void SetInteractable(bool interactable) => m_button.interactable = interactable;
    public void AddListener(UnityAction listener) => m_button.onClick.AddListener(listener);
    public void RemoveAllListeners() => m_button.onClick.RemoveAllListeners();
    
}
