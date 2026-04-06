using Sirenix.OdinInspector;
using UnityEngine;

public class ActionTimer : MonoBehaviour
{
    
}



public interface IAction
{
    public int Cost{get ; set;}

    public void RequiredConsume();
}
