using System;
using UnityEngine;

public class TextComponent : MonoBehaviour
{
    
    [SerializeField] private DynamicTextSetting m_textSetting;
    [SerializeField] private Vector3 m_offset = new Vector3(0,1.5f,0f) ;
    
    private string _currentText = "";
    private DynamicText _text;

    private IFocus _focus;

 
    private void Start()
    {
        _focus = GetComponent<IFocus>();
        _focus.OnFocus += SpawnText;
        _focus.OnUnfocus += DespawnText;
    }
    public void Init(string text)
    {
        _currentText = text;
    }

    private void OnDestroy()
    {
        _focus.OnFocus -= SpawnText;
        _focus.OnUnfocus -= DespawnText;
    }

    private void SpawnText()
    {
        _text = FlyweightFactory.Instance.Spawn<DynamicText>(m_textSetting, m_offset + transform.position,Quaternion.identity,transform);
        _text.SetText(_currentText,2f,Color.cyan);
        _ = _text.PlayTypeWriterEffect();
    }

    private void DespawnText()
    {
        if(!_text ) return;
        FlyweightFactory.Instance.Return(_text);
        _text =  null;
    }
}