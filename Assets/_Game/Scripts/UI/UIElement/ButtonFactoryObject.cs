using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField]  protected Button m_button;
    [SerializeField]  protected TMP_Text m_text;
    [SerializeField]  protected Image m_image;
    private ButtonAnimation _animation;
    
    private void Start()
    {
        TryGetComponent(out _animation);
    }
    public override void Despawn()
    {
       base.Despawn();
       m_button.onClick.RemoveAllListeners();
    }


    public void SetSprite(Sprite sprite)
    {
        if(m_image)m_image.sprite = sprite;
    }
    
    public void SetLocalScale(float scale) => m_button.transform.localScale = new Vector3(scale, scale, scale);
    public void SetText(string text) =>   m_text.text = text;
    


    public void MoveToLast() => transform.SetAsLastSibling();
    

    public void MoveToFirst() => transform.SetAsFirstSibling(); 

    public void SetInteractable(bool interactable) => m_button.interactable = interactable;
    public void AddListener(UnityAction listener) => m_button.onClick.AddListener(listener);

    public void RemoveAllListeners() => m_button.onClick.RemoveAllListeners();

    public void Center()
    {
        m_rectTransform.anchoredPosition = Vector2.zero; 
        m_rectTransform.localPosition = Vector3.zero;
        m_rectTransform.localRotation = Quaternion.identity;
    }
    public void PlayAnimation(bool show)
    {
        Debug.Log("Try Play" + gameObject.name);
        if(!_animation ) return;
        if (show) _animation.PlaySuccess();
        else _animation.PlayFail();
    }
    
}