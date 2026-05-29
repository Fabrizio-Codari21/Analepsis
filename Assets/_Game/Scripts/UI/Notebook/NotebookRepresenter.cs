using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using PrimeTween;
using Sirenix.OdinInspector;

public class NotebookRepresenter : MonoBehaviour,ITakeable
{
    [Header("Controller")]
    [ShowInInspector,ReadOnly] private NotebookManager  m_controller;
    [Header("Page Root")]
    [SerializeField] private Transform m_leftRoot;
    [SerializeField] private Transform m_rightRoot;
    [SerializeField] private Transform m_buttonSwitchLayoutRoot;
   
    
    [Header("Layout")]
    [SerializeField] private List<NotebookLayout> m_allLayout;
    
    [Header("UI Setting")]
    [SerializeField] private NotebookButton m_layoutButtonPrefab;
    public void Initialize(NotebookManager controller)
    {
        m_controller = controller;
        InitLayouts();
    }
    
    private void InitLayouts()
    {
        foreach (var layout in m_allLayout)
        { 
            layout.Initialize(m_leftRoot, m_rightRoot);
            m_controller.AddLayout(layout);   // Creo Layout
            
            var newButton = Instantiate(m_layoutButtonPrefab, m_buttonSwitchLayoutRoot);
            newButton.OnClick += () => m_controller.TryShowLayoutFor(layout);  // Crear button para swichtear a ese layout
        }
    }
  
    
    #region ITakeable
    public void TryTake(Transform takeRoot)
    {
        transform.SetParent(takeRoot,false);
        gameObject.SetActive(true);
        
    }
    
    public void Release()
    {
     
        transform.SetParent(null, false);
   
        gameObject.SetActive(false);
    }
    #endregion

   
}


