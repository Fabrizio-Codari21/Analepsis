using PrimeTween;
using UnityEngine;

public class PenduloAnimation : MonoBehaviour
{

    

    [SerializeField] private Vector3 m_startAngle;
    [SerializeField] private Vector3 m_endAngle;
    [SerializeField] private float duration;
    [SerializeField] private Ease m_ease = Ease.InOutSine;
    private void Start()
    {
        transform.localEulerAngles =  m_startAngle;

        Tween.LocalEulerAngles(transform,m_startAngle,m_endAngle,duration:duration,ease: m_ease,cycleMode:CycleMode.Yoyo,cycles:-1);
    }

   
}