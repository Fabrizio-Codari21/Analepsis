using Sirenix.OdinInspector;
using System;
using UnityEngine;

[System.Serializable]
public struct FlashbackInfo
{
    [TextArea(0,20)] public string info; //estaria bueno que esto se vea en escena sobre el personaje o algo asi.
    [PreviewField] public GameObject characterPrefab;
    public Transform flashbackTransform;

}
