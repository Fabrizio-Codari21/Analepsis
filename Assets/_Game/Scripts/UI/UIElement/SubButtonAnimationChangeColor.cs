using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimationChangeColor : ButtonAnimation
{
    
    [SerializeField] private Color m_changeColor;
    [SerializeField] private Color m_initialColor;
    [SerializeField] private Image m_image;


    public override void PlaySuccess()
    {
        Debug.Log("ButtonAnimationChangeColor.PlaySuccess");
        m_image.color = m_changeColor;
    }

    public override void PlayFail()
    {
        m_image.color = m_initialColor;
    }
}