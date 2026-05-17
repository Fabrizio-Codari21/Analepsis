using System;
using UnityEngine;

public class TouchLight : MonoBehaviour,ITouch
{
    [SerializeField] private Light m_light;


    public event Action OnFocus;
    public event Action OnUnfocus;
    public void Focus()
    {
      
    }

    public void Unfocus()
    {
    
    }

    public void Touch()
    {
        if(!m_light) return;
        m_light.enabled = !m_light.enabled;
    }
}