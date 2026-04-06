using System;
using UnityEngine;

public interface IInteractable : IFocus,ITipProvider
{
 
    public event Action OnStart;
    public event Action OnEnd;
    // esta diseñado de esta forma que sea flexible de puede decidir si necesita holdear un interactuable, o simple usra start o end. si no va holdea puede decidir usar solemente start or end (dependiendo la cual resulta mas comodo)
    public void InteractStart();  // cuando apenas tocas el botton
    public void InteractEnd(); // cuando soltar el botton
    
}
public interface IFocus
{
    public event Action OnFocus;
    public event Action OnUnfocus;
    public void Focus();
    public void Unfocus();
}

public interface ITipProvider
{
    string GetTip();
    
    
}

