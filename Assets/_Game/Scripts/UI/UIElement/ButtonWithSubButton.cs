using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonWithSubButton : ButtonFactoryObject
{
    [SerializeField] private Transform m_subButtonRoot;
    [SerializeField] private ButtonSetting m_subButtonSetting;
 
    private readonly List<IFlyweight> _myChildren = new List<IFlyweight>();


   

    public override void Despawn()
    {
        base.Despawn();
        foreach (IFlyweight child in _myChildren)
        {
            FlyweightFactory.Instance.Return(child);
        }
        
        _myChildren.Clear();
    }

    public ButtonFactoryObject AddSubButton()
    {
        var sub = FlyweightFactory.Instance.Spawn<ButtonFactoryObject>(m_subButtonSetting,Vector3.zero, Quaternion.identity, m_subButtonRoot);
        
        
        _myChildren.Add(sub);

        return sub;
    }


    
}

public abstract class ButtonAnimation : MonoBehaviour
{
    public abstract void PlaySuccess();
    public abstract void PlayFail();
}