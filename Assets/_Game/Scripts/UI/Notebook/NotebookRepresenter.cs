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
    
    private readonly List<NotebookLayout> _layouts = new();
    private NotebookLayout _currentLayout;
    private int _currentIndex;
    
    
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

    #region Page Control
    
    #region Switch
    public void NextPage()
    {
        if (_layouts.Count == 0) return;

        int next = _currentIndex + 1;

        if (next >= _layouts.Count) next = 0;

        ShowLayout(next);
    }

    public void PreviousPage()
    {
        if (_layouts.Count == 0) return;

        int prev = _currentIndex - 1;

        if (prev < 0) prev = _layouts.Count - 1;

        ShowLayout(prev);
    }
    
    #endregion
    
    #endregion
    
    #region Layout
    public void AddLayout(NotebookLayout layout)
    {
        layout.Index = _layouts.Count;
        layout.Initialize(m_leftRoot, m_rightRoot);

        _layouts.Add(layout);
    }

    public void ShowLayout(int index)
    {
        if(_layouts.Count == 0) return;
        if (index < 0 || index >= _layouts.Count) return;
        
        _currentLayout?.Hide();
        _currentIndex =  index;
        _currentLayout = _layouts[index];
        _currentLayout.Show();
        
    }

    #endregion
}
