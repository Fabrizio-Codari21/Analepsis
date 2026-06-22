using UnityEngine;

[CreateAssetMenu(menuName = "Game/UI Element/DynamicText",fileName = "DynamicText")]
public class DynamicTextSetting : FlyweightSetting
{
    public float size;
    public Color color;
    public bool Animated = true;
    public float Speed = 500f;
}