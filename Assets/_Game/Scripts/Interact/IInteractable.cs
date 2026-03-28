using UnityEngine;

public interface IInteractable : IFocus
{
    public GameObject InteractableObject { get; }
    
    
    // esta diseñado de esta forma que sea flexible de puede decidir si necesita holdear un interactuable, o simple usra start o end. si no va holdea puede decidir usar solemente start or end (dependiendo la cual resulta mas comodo)
    public void InteractStart();  // cuando apenas tocas el botton
    public void InteractEnd(); // cuando soltar el botton
    
    
}

public interface IFocus
{
    public void Focus();
    public void Unfocus();
}