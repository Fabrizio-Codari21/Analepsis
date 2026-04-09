using UnityEngine;

public interface ITipProvider
{
    public string GetTip();
    
    public void AddTip(Tip tip);
    public void RemoveTip(Tip tip);
    
}
