using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField]  protected Button m_button;
    [SerializeField]  protected TMP_Text m_text;
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
    
    public void SetText(string text) =>   m_text.text = text;
    


    public void MoveToLast() => transform.SetAsLastSibling();
    

    public void MoveToFirst() => transform.SetAsFirstSibling(); 

    public void SetInteractable(bool interactable) => m_button.interactable = interactable;
    public void AddListener(UnityAction listener) => m_button.onClick.AddListener(listener);

    public void RemoveAllListeners() => m_button.onClick.RemoveAllListeners();
    
    public void PlayAnimation(bool show)
    {
        Debug.Log("Try Play" + gameObject.name);
        if(!_animation ) return;
        if (show) _animation.PlaySuccess();
        else _animation.PlayFail();
    }
    
}