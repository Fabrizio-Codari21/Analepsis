using UnityEngine;
using UnityEngine.UI;
public class UIImage : FactoryUIObject
{
    
    [SerializeField] private Image m_image;

    
    
    public void SetImage(Sprite sprite ,float factor = 1)
    {
        if (factor == 0) factor = 1;
        m_image.sprite = sprite;
        m_image.SetNativeSize();
        Vector2 nativeSize = m_image.rectTransform.sizeDelta;
        m_image.rectTransform.sizeDelta = nativeSize / factor;
    }
}