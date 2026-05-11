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

    public void SetRandomSprite() => 
        baseImage.sprite = possibleSprites[Random.Range(0, possibleSprites.Length - 1)];

    public List<int> CalculateRotation(int amount)
    {
        switch (amount)
        {
            case 1: return new() { 0 };
            case 2: return new() { -20, 20 };
            case 3: return new() { -30, 0, 30 };
            default: return new() { 0 };
        }
    }

    public void SetRotationOnGroup(int index, int amount)
    {
        var values = CalculateRotation(amount);
        baseImage.transform.Rotate(0, 0, values[index]);
    }
    
}
