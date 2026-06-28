using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class Aim : MonoBehaviour
{
    [SerializeField] private TextureAnimationEvent m_animationEvent;
    [SerializeField] private TwoDimentionAnimation m_defaultAim;
    [SerializeField] private EventChannel m_resetAim;
    [SerializeField] private Image m_aim;
    private Sequence _sequence = default;

    private void Start()
    {
       
        PlayAim();
    }

    private void OnEnable()
    {
        m_resetAim.OnEventRaised += PlayAim;
        m_animationEvent.OnEventRaised += PlayAim;
    }

    private void OnDisable()
    {
        m_resetAim.OnEventRaised -= PlayAim;
        m_animationEvent.OnEventRaised -= PlayAim;
    }

    private void PlayAim() => PlayAim(m_defaultAim);
    
    private void PlayAim(TwoDimentionAnimation animations)
    {
        if(_sequence.isAlive) _sequence.Stop();
        if (animations == null || animations.animationSheets == null || animations.animationSheets.Length <= 0) return;
        m_aim.rectTransform.sizeDelta = animations.m_size;
        if (animations.animationSheets.Length == 1)
        {
             m_aim .sprite = animations.animationSheets[0];
            return;
        }
        
  
        _sequence = Sequence.Create(cycles: -1);
        float frameTime = 1f / animations.frameRate;
        foreach (var s in animations.animationSheets)
        {
            var current = s;
            _sequence.ChainCallback(() => { m_aim.sprite = current; });
            _sequence.ChainDelay(frameTime);
        }
    }

}