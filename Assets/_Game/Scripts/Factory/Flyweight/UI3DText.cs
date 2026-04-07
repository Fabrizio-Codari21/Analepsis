using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UI3DText : FactoryUIObject
{
    [SerializeField]
    private TMP_Text m_text;
    
    
    [Button]
    public void CalculateWidthAndHeight()
    {
        Vector2 prefSize = m_text.GetPreferredValues(m_text.text);
        m_rectTransform.sizeDelta = new Vector2(prefSize.x, prefSize.y);
        m_rectTransform.anchoredPosition = new Vector2(0, 0);
    }
    
    
    
}