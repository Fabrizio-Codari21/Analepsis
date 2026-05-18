using UnityEngine;
using UnityEngine.UI;
public class UIImage : FactoryUIObject
{
    
    [SerializeField] private Image m_image;
    
    [SerializeField] private int Factor = 1;
    
    
    
    public void SetImage(Sprite sprite)
    {

        if (Factor == 0) Factor = 1;
        m_image.sprite = sprite;
        m_image.SetNativeSize();
        
        Vector2 nativeSize = m_image.rectTransform.sizeDelta;
        m_image.rectTransform.sizeDelta = nativeSize / Factor;
    }
}