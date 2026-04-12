using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
//using System.ComponentModel;

public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField]  protected Button m_button;
    [SerializeField]  protected TextMeshProUGUI m_text;

    // estas no habria que asignarlas en el inspector en teoria
    [SerializeField, HideInInspector] protected Dictionary<TheoryboardManager.Whodunnit, TheoryPanel> _boardTransforms;
    [SerializeField, HideInInspector] protected TheoryboardView _view;
    [SerializeField, ShowInInspector, ReadOnly] protected List<TheoryboardManager.Whodunnit> proof = new();

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
    public void SetBoard(Dictionary<TheoryboardManager.Whodunnit, TheoryPanel> board) => _boardTransforms = board;
    public void SetView(TheoryboardView view) => _view = view;

    public void SetProof(List<TheoryboardManager.Whodunnit> isProof) => proof = isProof;
    public List<TheoryboardManager.Whodunnit> GetProof() => proof;
}