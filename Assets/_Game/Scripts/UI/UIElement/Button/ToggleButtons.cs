using System;
using System.Collections.Generic;
using UnityEngine;

public class ToggleButtons : ButtonFactoryObject
{
    [SerializeField] private ButtonSetting m_subButtonSetting;
    [SerializeField] private Transform m_subButtonRoot;

    [SerializeField] private Color m_normalColor = Color.gray;
    [SerializeField] private Color m_activeColor;
    
    
    private List<ButtonFactoryObject> _activeSubButtons = new();
    public override void Despawn()
    {
        foreach (var sub in _activeSubButtons)
        {
            if (sub != null) FlyweightFactory.Instance.Return(sub);
        }
        _activeSubButtons.Clear();
        base.Despawn();
    }

    public void AddToggleButton(Action doAction,Action undoAction,bool isToggled = false)
    {
        var button = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(
            m_subButtonSetting,
            Vector3.zero,
            Quaternion.identity,
            m_subButtonRoot
        );
        button.SetInteractable(true);
        button.MoveToLast();
        
        _activeSubButtons.Add(button);

        bool toggled = isToggled;
        button.SetImageColor(toggled ? m_activeColor : m_normalColor);
        
        button.RemoveAllListeners();
        
        button.AddListener(() =>
        {
            if (!toggled)
            {
                
                toggled = true;
                button.SetImageColor(m_activeColor);
                doAction();
            }
            else
            {
                toggled = false;
                button.SetImageColor(m_normalColor);
                undoAction();
            }
        });
    }
    
}