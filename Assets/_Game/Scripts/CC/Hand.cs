using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class Hand : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private Transform m_handRoot;
    [Header("Rendering Camera")]
    [SerializeField] private Camera m_handCamera;

    [Header("Channels")]
    [SerializeField] private TakeableEvent takeOutChannel;
    [SerializeField] private TakeableEvent putInChannel;

    [Header("Animation")]
    [SerializeField] private Animator m_animator;
    [Range(0f,1f)][SerializeField] private float m_takeCrossFade = 0.2f;
    [Range(0f,1f)][SerializeField] private float m_putCrossFade = 0.2f;
    
    private const string TakeoutState = "Anim_Hand_Takeout";  // despues pueder cambiar por int , que es un poco mas eficiente que string
    private const string PutinState = "Anim_Hand_Put";

    
    private void Start()
    {
        takeOutChannel.OnEventRaised += Takeout;
        putInChannel.OnEventRaised += Put;
        
        m_handCamera.gameObject.SetActive(false);
        
        
    }

    
    private void Takeout(ITakeable objectToTakeOut)
    {
        m_handCamera.gameObject.SetActive(true);
        
        objectToTakeOut.TryTake(m_handRoot);
        m_animator.CrossFade(TakeoutState, m_takeCrossFade);
    }


    private void Put(ITakeable objectToPutIn)
    {
        PlayAnimationAsync(PutinState,m_putCrossFade, callBack: ()=>
        {
            objectToPutIn.Release();
            m_handCamera.gameObject.SetActive(false);
        }).Forget();
    }


    private async UniTask PlayAnimationAsync(string hash,float transitionDuration,Action callBack)  // animation hand
    {
        m_animator.CrossFade(hash, transitionDuration);
        
        await UniTask.Yield();
        var animationStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
        await UniTask.Delay(TimeSpan.FromSeconds(animationStateInfo.length));
        
        callBack();
    }
}

public interface ITakeable
{
    public void TryTake(Transform takeRoot);
    
    public void Release();
    
}