using UnityEngine;
using UnityEngine.UI;
public class TagButton : ButtonFactoryObject
{
    [SerializeField] private Image m_image;
   
    public void MarkTag(bool wasUnlocked)
    {
        m_image.gameObject.SetActive(wasUnlocked);
    }
}