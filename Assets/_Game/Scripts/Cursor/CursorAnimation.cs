using UnityEngine;

[CreateAssetMenu(fileName = "Cursor Animation", menuName = "Game/Cursor Animation/Animation")]
public class CursorAnimation :ScriptableObject
{
    public Texture2D[] animationSheets;
    public float frameRate = 8f;
}