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

public class NotebookRepresenter : MonoBehaviour,ITakeable
{
    [Header("Page Root")]
    [SerializeField] private Transform m_leftRoot;
    [SerializeField] private Transform m_rightRoot;
    
    public event Action<PageType> OnPageSwitchRequested =  delegate { };
    
    public void TryTake(Transform takeRoot)
    {
       
        transform.SetParent(takeRoot,false);
        gameObject.SetActive(true);
        
    }
    
    public void RequestPage(PageType pageType) => OnPageSwitchRequested?.Invoke(pageType);
    public void Release()
    {
     
        transform.SetParent(null, false);
   
        gameObject.SetActive(false);
    }
}

public class NotebookSwitchButton : MonoBehaviour
{
    [SerializeField] private NotebookRepresenter m_notebookRepresenter;
    [SerializeField] private Button m_button;
    [SerializeField] private PageType m_pageType;
 

    private void OnDisable()
    {
        m_button.onClick.RemoveAllListeners();
    }
}
