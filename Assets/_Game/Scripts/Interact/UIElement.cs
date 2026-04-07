using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class UIElement
{
    public static async UniTask<DynamicUIText> PlayDynamicText(
        string text,
        DynamicTextSetting setting,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            var t = FlyweightFactory.Instance.Spawn<DynamicUIText>(
                setting,
                position,
                rotation,
                parent);

            t.SetText(text,setting.size,setting.color);

            await UniTask.NextFrame(token);
            token.ThrowIfCancellationRequested();

            await t.PlayTypeWriterEffect(externalToken: token);
            return t;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}