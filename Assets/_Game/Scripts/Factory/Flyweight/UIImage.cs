using UnityEngine;
using UnityEngine.UI;
public class UIImage : FactoryUIObject
{
    
    [SerializeField] private Image m_image;
    
    public void SetImage(Sprite sprite)
    {
        m_image.sprite = sprite;
        m_image.SetNativeSize();
    }
}