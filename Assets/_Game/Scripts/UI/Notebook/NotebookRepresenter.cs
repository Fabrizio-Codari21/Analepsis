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
    
   

    
    
    
    [SerializeField] private CharacterLayout m_characterLayout;



    public event Action<PageType> OnPageSwitchRequested =  delegate { };
 

    public void RequestPage(PageType pageType)
    {
        
        OnPageSwitchRequested?.Invoke(pageType);
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


