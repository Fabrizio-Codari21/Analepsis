using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class FillMarkButton : ButtonFactoryObject
{
    [SerializeField] protected Image m_buttonImage;  
    public async UniTask PlayImageFill(float fill,float duration = 0.5f, Color color = default)
    {
        if (m_buttonImage == null) return;
        Tween.StopAll(m_buttonImage.gameObject);
        if(color != default) m_buttonImage.color = color;
        var seq = Sequence.Create();
        _ = seq.Group(Tween.UIFillAmount(m_buttonImage, fill, duration, Ease.OutQuint));
        await seq;
    }
}