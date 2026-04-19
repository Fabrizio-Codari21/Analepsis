using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;
public class UITransitionEffect : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [Header("Fade In")]
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;
    [SerializeField,Min(0.05f)] private float fadeInDuration;
    [Header("Fade Out")]
    [SerializeField] private Ease fadeOutEase = Ease.InQuad;
    [SerializeField,Min(0.05f)] private float fadeOutDuration;
    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }



    public async UniTask FadeIn()
    {
        canvasGroup.gameObject.SetActive(true);

        Tween.StopAll(canvasGroup);
        await Tween.Alpha(canvasGroup, endValue: 1f, fadeInDuration, ease: fadeInEase);
    }
    
    public async UniTask FadeOut()
    {
 
        Tween.StopAll(canvasGroup);
        await Tween.Alpha(canvasGroup, endValue: 0f,fadeOutDuration, ease: fadeOutEase);
        canvasGroup.gameObject.SetActive(false);
    }
    
    
}

