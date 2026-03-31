using TMPro;
using UnityEngine;
public class DynamicText : FactoryUIObject
{
    [SerializeField] private TMP_Text m_text;
    public override void OnSpawn()
    {
       gameObject.SetActive(true);
    }
    public void SetText(string text)
    {
        m_text.text = text;
    }
}