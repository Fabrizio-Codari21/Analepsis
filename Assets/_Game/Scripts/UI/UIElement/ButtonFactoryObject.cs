using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PrimeTween;


public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField]  protected Button m_button;
    [SerializeField]  protected TMP_Text m_text;
    
    public override void Despawn()
    {
       base.Despawn();
       m_button.onClick.RemoveAllListeners();
    }
   

    public void SetText(string text) =>   m_text.text = text;
    


    public void MoveToLast() => transform.SetAsLastSibling();
    

    public void MoveToFirst() => transform.SetAsFirstSibling(); 

    public void SetInteractable(bool interactable) => m_button.interactable = interactable;
    public void AddListener(UnityAction listener) => m_button.onClick.AddListener(listener);

    public void RemoveAllListeners() => m_button.onClick.RemoveAllListeners();

 
}

public class FillMarkButton : ButtonFactoryObject
{
    [SerializeField] protected Image m_buttonImage;  
    public async UniTask PlayImageFill(float fill,float duration = 0.5f, Color color = default)
    {
        if (m_buttonImage == null) return;
        Tween.StopAll(m_buttonImage.gameObject);
        if(color != default) m_buttonImage.color = color;
        var seq = Sequence.Create();
        _ = seq.Group(Tween.UIFillAmount(m_buttonImage, fill, duration, Ease.OutQuint));
        await seq;
    }
}