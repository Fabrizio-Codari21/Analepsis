using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField]  private Button m_button;
    [SerializeField]  private TMP_Text m_text;
    public override void Despawn()
    {
       base.Despawn();
       m_button.onClick.RemoveAllListeners();
    }

    
    public void SetText(string text)
    {
        m_text.text = text;
    }
    
    public void SetInteractable(bool interactable) => m_button.interactable = interactable;
    public void AddListener(UnityAction listener) => m_button.onClick.AddListener(listener);
    public void RemoveAllListeners() => m_button.onClick.RemoveAllListeners();
}