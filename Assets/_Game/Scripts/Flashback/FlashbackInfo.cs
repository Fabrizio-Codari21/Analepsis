using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class FlashbackInfo
{
    [TextArea(0,20)] public string info; //estaria bueno que esto se vea en escena sobre el personaje o algo asi.
    [PreviewField] public Interactable characterPrefab;
    public TransformKey key;
    public Vector3 offset;
    
}