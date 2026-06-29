using System;
using UnityEngine;

[CreateAssetMenu(fileName = "2D Sprite Animation", menuName = "Game/2D Animation/Texture")]
public class CustomCursor : ScriptableObject
{
    public Vector2 m_skewedVector =  Vector2.one;
    [Header("Loop States")]
    public Texture2D[] animationSheetsUp;       
    public Texture2D[] animationSheetsDown;    

    [Header("Transition States")]
    public Texture2D[] transitionToDown;      
    public Texture2D[] transitionToUp;
    public float frameRate = 8f;

    [Header("SFX")]
    public string clickSound;
    public string upSound;
    public string hoverSound;

}