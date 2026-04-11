using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField]  protected Button m_button;
    [SerializeField]  protected TextMeshProUGUI m_text;

    // estas no habria que asignarlas en el inspector en teoria
    [SerializeField] protected Transform _boardTransform;
    [SerializeField] protected TheoryboardView _view;

    public override void Despawn()
    {
       base.Despawn();
       m_button.onClick.RemoveAllListeners();
    }

    public void SetText(string text)
    {
        m_text.text = text;
    }

    public void MoveToLast()
    {
        transform.SetAsLastSibling();
    }

    public void MoveToFirst() { transform.SetAsFirstSibling(); }
    
    public void SetInteractable(bool interactable) => m_button.interactable = interactable;
    public void AddListener(UnityAction listener) => m_button.onClick.AddListener(listener);
    public void RemoveAllListeners() => m_button.onClick.RemoveAllListeners();
    public void SetBoard(Transform board) => _boardTransform = board;
    public void SetView(TheoryboardView view) => _view = view;
}