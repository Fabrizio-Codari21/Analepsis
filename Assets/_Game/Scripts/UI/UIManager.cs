using System;
using JetBrains.Annotations;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject[] m_uIObjects;
    private void Start()
    {
        foreach (GameObject obj in m_uIObjects)
        {
            Instantiate(obj,transform);
        }
    }
}
