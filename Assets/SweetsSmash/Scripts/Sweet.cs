using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public struct Sweets
{
    public string SweetName;
    public Color SweetColor;
    public Sprite SweetSprite;
    private int SweetId;
}

public class Sweet : MonoBehaviour
{
    public Sweets[] sweets;
    public int SweetID = 0;
    private Color originalColor;

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public void SetType(int randomIndex)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer is null)
        {
            Debug.LogError("No SpriteRenderer found on this GameObject!");
            return;
        }

        SweetID = randomIndex;
        Sweets selectedSweet = sweets[randomIndex];

        spriteRenderer.sprite = selectedSweet.SweetSprite;
        spriteRenderer.color = selectedSweet.SweetColor;
        originalColor = selectedSweet.SweetColor;
        this.name = selectedSweet.SweetName;
    }
    
    public void SetPosition(int x, int y)
    {
        GridX = x;
        GridY = y;
    }
    
    public Color GetOriginalColor()
    {
        return originalColor;
    }
}