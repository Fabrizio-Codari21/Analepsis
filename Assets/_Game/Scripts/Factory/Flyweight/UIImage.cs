using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class UIImage : FactoryUIObject
{
    
    [SerializeField] private Image m_image;

    
    
    public void SetImage(Sprite sprite ,float factor = 1)
    {
        if (factor == 0) factor = 1;
        m_image.sprite = sprite;
        
        var images = GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            //image.SetNativeSize();
            Vector2 nativeSize = image.rectTransform.sizeDelta;
            image.rectTransform.sizeDelta = nativeSize * factor;
        }
    }
}