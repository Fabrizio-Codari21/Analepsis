using System;
using UnityEngine;
using UnityEngine.UI;

public class NotebookButton : MonoBehaviour
{
    [SerializeField] private Button m_button;

    [SerializeField] private Image m_icon;
    public event Action OnClick =  delegate { };
    
    private void Start()
    {
        m_button.onClick.AddListener(()=>
        {
           OnClick?.Invoke();
        });
    }


    public void SetImage(Sprite sprite)
    {
        m_icon.sprite = sprite;
    }
}