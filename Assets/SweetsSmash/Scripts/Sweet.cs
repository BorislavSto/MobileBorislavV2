using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public enum SweetName
{
    RockCandy,
    Chocolate,
    Pepermint,
    Gumdrop,
}

[Serializable]
public struct Sweets
{
    public SweetName SweetName;
    public Color SweetColor;
    public Sprite SweetSprite; 
}

public class Sweet : MonoBehaviour
{
    public Sweets[] sweets;
    public SweetName originalSweetName;
    private Color originalColor;  // Store the original color
    
    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer is null)
        {
            Debug.LogError("No SpriteRenderer found on this GameObject!");
            return;
        }

        int randomIndex = Random.Range(0, sweets.Length);
        originalSweetName = sweets[randomIndex].SweetName;
        Sweets selectedSweet = sweets[randomIndex];

        spriteRenderer.sprite = selectedSweet.SweetSprite;
        spriteRenderer.color = selectedSweet.SweetColor;
        originalColor = selectedSweet.SweetColor;
        this.name = selectedSweet.SweetName.ToString();

        Debug.Log($"Selected Candy: {selectedSweet.SweetName}");
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
