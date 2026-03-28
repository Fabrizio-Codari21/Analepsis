using UnityEngine;

public interface IFlyweight
{
    GameObject GO { get; }
    FlyweightSetting Setting { get; set; }
    
}
