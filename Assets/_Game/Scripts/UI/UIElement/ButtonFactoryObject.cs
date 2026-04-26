using TMPro;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PrimeTween; 

public class ButtonFactoryObject : FactoryUIObject
{
    [SerializeField]  protected Button m_button;
    [SerializeField]  protected Button subButton;
    [SerializeField]  protected TextMeshProUGUI m_text;

    protected bool m_isOpen;
    protected bool m_isCharacter;
    protected List<ButtonFactoryObject> m_childrenButtons = new();

    // estas no habria que asignarlas en el inspector en teoria
    [SerializeField, HideInInspector] protected Dictionary<Whodunnit, TheoryPanel> _boardTransforms;
    [SerializeField, HideInInspector] protected TheoryboardView _view;
    [SerializeField, ShowInInspector, ReadOnly] protected List<Whodunnit> proof = new();
    [SerializeField] protected Image m_buttonImage;
    public override void Despawn()
    {
       base.Despawn();
       m_button.onClick.RemoveAllListeners();
       if(subButton) subButton.onClick.RemoveAllListeners();
       if(_parent != null) RemoveFromParent(); 
       ClearChildren();
    }

    public void SetFill(float fill)
    {
        if(m_buttonImage) m_buttonImage.fillAmount = fill; 
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
        m_text.text = text;
    }

    public void MoveToLast()
    {
        transform.SetAsLastSibling();
    }

    public void MoveToFirst() { transform.SetAsFirstSibling(); }
    public void MoveToPosition(int position) { transform.SetSiblingIndex(position); }
    public int GetPosition() { return transform.GetSiblingIndex(); }

    public void DisableSub() => subButton?.gameObject.SetActive(false);

    public void EnableSub(bool enabled = true) => subButton?.gameObject.SetActive(enabled);
    public void MoveSubToLast() => subButton?.gameObject.transform.SetAsLastSibling();
    public void DisplayMark(bool marked) 
        => subButton.GetComponent<Image>().color = marked ? Color.yellow : Color.gray;

    
    public void SetInteractable(bool interactable) => m_button.interactable = interactable;
    public void AddListener(UnityAction listener) => m_button.onClick.AddListener(listener);
    public void AddListenerToSub(UnityAction listener) => subButton?.onClick.AddListener(listener);
    public void RemoveAllListeners() => m_button.onClick.RemoveAllListeners();
    public void RemoveAllListenersFromSub() => subButton?.onClick.RemoveAllListeners();
    public void SetBoard(Dictionary<Whodunnit, TheoryPanel> board) => _boardTransforms = board;
    public void SetView(TheoryboardView view) => _view = view;

    public void SetProof(List<Whodunnit> isProof) => proof = isProof;
    public List<Whodunnit> GetProof() => proof;
    public bool IsCharacter() => m_isCharacter;
    public void SetCharacter(bool isCharacter) => m_isCharacter = isCharacter;

    protected ButtonFactoryObject _parent;  
    public bool IsOpen() => m_isOpen;
    public void MakeOpen(bool open) => m_isOpen = open;
    public ButtonFactoryObject GetParent() => _parent;
    public void AddToChildren(ButtonFactoryObject child)
    {
        m_childrenButtons.Add(child);
        child._parent = this;
    }
    public void RemoveFromParent() => _parent?.m_childrenButtons?.Remove(this);
    public void ClearChildren()
    {
        foreach(var child in m_childrenButtons) Destroy(child.gameObject);
        m_childrenButtons.Clear();
    } 
}