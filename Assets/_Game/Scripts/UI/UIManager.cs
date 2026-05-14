using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject[] m_uIObjects;

    [InfoBox("Make sure that the bars' scale is not below 0.6")]
    [SerializeField] private Transform m_aspectRatioBars;

    public float AspectRatioOffset(float max = 540f) =>
        Mathf.Lerp(max, 0, Mathf.Clamp(m_aspectRatioBars.localScale.y, 0.6f, 1));

    public float AspectRatioScale(float min = 0f) =>
        Mathf.Lerp(min, 1, Mathf.Clamp(m_aspectRatioBars.localScale.y, 0.6f, 1));

    private void Start()
    {
        m_aspectRatioBars.gameObject.SetActive(true);
        foreach (GameObject obj in m_uIObjects)
        {
            Instantiate(obj,transform);
        }
    }
}
