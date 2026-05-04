using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSelector : MonoBehaviour
{
    public Image baseImage;
    public Sprite[] possibleSprites = new Sprite[4];

    public Sprite GetSpriteBasedOnSize(int size)
    {
        if (possibleSprites.Length <= 0 || size <= 0) return null;

        if(size <= 2) return possibleSprites[0];
        else if(size <= 4) return possibleSprites[1];
        else if(size <= 8) return possibleSprites[2];
        else return possibleSprites[3];
    }

    public void SetSprite(int size)
    {
        baseImage.sprite = GetSpriteBasedOnSize(size);
    }
    
}
