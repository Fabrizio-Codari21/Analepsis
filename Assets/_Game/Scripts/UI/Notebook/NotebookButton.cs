using System;
using UnityEngine;
using UnityEngine.UI;

public class NotebookButton : MonoBehaviour
{
    [SerializeField] private Button m_button;
    
    public event Action OnClick =  delegate { };
    
    private void Start()
    {
        m_button.onClick.AddListener(()=>
        {
           OnClick?.Invoke();
        });
    }
}