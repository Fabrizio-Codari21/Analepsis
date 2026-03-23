using System;
using UnityEngine;

public class ControllerSpawn : MonoBehaviour
{
    [SerializeField] private Transform m_spawnPoint;

    [SerializeField] private GameObject  m_prefab;
    private void Start()
    {
        Instantiate(m_prefab, m_spawnPoint.position, m_spawnPoint.rotation);
        
    }
}
