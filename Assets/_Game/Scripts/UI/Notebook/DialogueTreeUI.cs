using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTreeUI : MonoBehaviour
{

    [SerializeField] private Transform m_detailRoot;
    [SerializeField] private DynamicTextSetting  m_dynamicTextSetting;
    [SerializeField] private ScrollRect m_scrollRect;
    [Range(10f,700f)]
    [SerializeField] private float m_textWidth = 600f;
    
    public async UniTask PlayText(List<string> text, CancellationToken token, Transform parent = null, float sizeOverride = 0) 
    {
        token.ThrowIfCancellationRequested();

        foreach (var item in text)
        {
            if(item == null) return;
            var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
                m_dynamicTextSetting,
                Vector3.zero,
                Quaternion.identity,
                parent != null ? parent : m_detailRoot);

            t.SetText(
                item, 
                !Mathf.Approximately(sizeOverride, 0) ? sizeOverride : m_dynamicTextSetting.size,
                m_dynamicTextSetting.color, 
                m_textWidth, 
                true, 
                item == text.Last() ? 11 : 0);

            t.ToLast();
            await UniTask.NextFrame(token);
            token.ThrowIfCancellationRequested();
            await t.PlayTypeWriterEffect(externalToken: token);
        }

    }
    
}