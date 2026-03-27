using System;
using UnityEngine;

public class UIManager : RegulatorSingleton<UIManager>
{
    [SerializeField] private Transform m_optionRoot;
    
    [SerializeField] private EventChannel m_popEventChannel;

    [SerializeField] private IActivityEvent m_pushEventChannel;
    
}
