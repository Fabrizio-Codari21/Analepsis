using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable : IFocus,ITipProvider
{
 
    public event Action OnStart;
    public event Action OnEnd;
    // esta diseñado de esta forma que sea flexible de puede decidir si necesita holdear un interactuable, o simple usra start o end. si no va holdea puede decidir usar solemente start or end (dependiendo la cual resulta mas comodo)
    public void InteractStart();  // cuando apenas tocas el botton
    public void InteractEnd(); // cuando soltar el botton
    
}