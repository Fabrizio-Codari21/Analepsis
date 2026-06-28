using UnityEngine;

[CreateAssetMenu(fileName = "2D Sprite Animation", menuName = "Game/2D Animation/Sprite")]
public class TwoDimentionAnimation :ScriptableObject
{
    
    [Tooltip("Size Per pixer / adj per 1920 * 1080")]
    public Vector2 m_size =  Vector2.one;
    public Sprite[] animationSheets;
    public float frameRate = 8f;
}