using System;
using UnityEngine;

public class AimAnimationProvider : MonoBehaviour
{
    private IFocus _focus;

    [SerializeField] private TwoDimentionAnimation m_twoDimensionAnimation;
    [SerializeField] private EventChannel m_resetAim;
    [SerializeField] private TextureAnimationEvent m_animation2D;

    private void Start()
    {
        _focus = GetComponent<IFocus>();
        _focus.OnFocus += Focus;
        _focus.OnUnfocus += ResetAim;
    }


    private void OnDestroy()
    {
        _focus.OnFocus -= Focus;
        _focus.OnUnfocus -= ResetAim;
    }

    private void ResetAim()
    {
        Debug.Log("ResetAim");
        m_resetAim.Raise();
    }

    private void Focus()
    {
        Debug.Log("Focus");
        m_animation2D.Raise(m_twoDimensionAnimation);
    }

}