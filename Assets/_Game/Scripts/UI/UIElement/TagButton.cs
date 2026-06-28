using UnityEngine;
using UnityEngine.UI;
public class TagButton : ButtonFactoryObject
{
    
   
    public void MarkTag(bool wasUnlocked)
    {
        m_image.gameObject.SetActive(wasUnlocked);
    }
}