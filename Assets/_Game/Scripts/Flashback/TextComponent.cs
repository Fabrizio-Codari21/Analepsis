using Cysharp.Threading.Tasks;
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
        _focus.OnFocus += TryToSpawnText;
        _focus.OnUnfocus += DespawnText;
    }
    public void Init(string text)
    {
        _currentText = text;
    }

    private void OnDestroy()
    {
       
        _focus.OnFocus -= TryToSpawnText;
        _focus.OnUnfocus -= DespawnText;
    }

    private void TryToSpawnText() => _ = SpawnText(); 
    private async UniTask SpawnText()
    {
        _text = FlyweightFactory.Instance.Spawn<DynamicText>(
            m_textSetting, 
            m_offset + transform.position,
            Quaternion.identity,
            transform);

        _text.SetText(_currentText,2f,Color.cyan);
        await _text.PlayTypeWriterEffect();
        //
        // if(!_exitText) _exitText = FlyweightFactory.Instance.Spawn<DynamicUIText>(
        //     m_textSetting,
        //     new Vector3(0,-400,0),
        //     Quaternion.identity);
        //
        // _exitText.SetText("[Press 'F' to leave the flashback.]", 2f, Color.cyan);
        // await _exitText.PlayTypeWriterEffect();
    }

    private void DespawnText()
    {
        if(!_text ) return;
        FlyweightFactory.Instance.Return(_text);
        _text =  null;
    }
}