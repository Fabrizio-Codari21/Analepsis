using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(BoneRenderer))]
public class AutoBoneRender : MonoBehaviour
{
    [SerializeField] private BoneRenderer _boneRenderer;

    [SerializeField] private Transform _root;
    private void OnValidate()
    {
        if(_root == null) return;
        if(_boneRenderer == null) _boneRenderer = GetComponent<BoneRenderer>();

        Transform[] allBones = _root.GetComponentsInChildren<Transform>(true);
        
        _boneRenderer.transforms = allBones;
    }
}
