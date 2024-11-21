using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BlockoutArea
{
    public Vector2 blockMin;
    public Vector2 blockMax;
}

public class GridManager : MonoBehaviour
{
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float gridSpacing = 0.6f;
    public GameObject candyPrefab;
    public Transform gridParent;
    
    [Header("Blockout Area")]
    public BlockoutArea[] blockoutAreas;
    
    [HideInInspector]
    public GameObject[,] grid;
    
    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        InitializeGrid();
    }
    
    void InitializeGrid()
    {
        Vector2 parentPosition = gridParent.position;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsWithinBlockoutArea(x, y)) 
                    continue;
                
                Vector2 position = parentPosition + new Vector2(x * gridSpacing, y * gridSpacing);
                GameObject candy = Instantiate(candyPrefab, position, Quaternion.identity, gridParent);
                grid[x, y] = candy;
                candy.GetComponent<Sweet>().SetPosition(x,y);
            }
        }
    }
    
    bool IsWithinBlockoutArea(int x, int y)
    {
        foreach (BlockoutArea blockoutArea in blockoutAreas)
        {
            if (x >= blockoutArea.blockMin.x && x <= blockoutArea.blockMax.x &&
                y >= blockoutArea.blockMin.y && y <= blockoutArea.blockMax.y)
            {
                return true;
            }
        }
        return false;
    }

    public void CheckForMatches()
    {
        // Create a HashSet to store candies to destroy (prevents duplicates)
        HashSet<GameObject> candiesToDestroy = new HashSet<GameObject>();

        // Horizontal matches
        for (int x = 0; x < gridWidth - 2; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject candy = grid[x, y];
                if (candy == null) continue;

                Sweet candyScript = candy.GetComponent<Sweet>();

                // Check for a horizontal match
                if (grid[x + 1, y]?.GetComponent<Sweet>().originalSweet == candyScript.originalSweet &&
                    grid[x + 2, y]?.GetComponent<Sweet>().originalSweet == candyScript.originalSweet)
                {
                    // Add candies in the match to the HashSet
                    candiesToDestroy.Add(grid[x, y]);
                    candiesToDestroy.Add(grid[x + 1, y]);
                    candiesToDestroy.Add(grid[x + 2, y]);
                }
            }
        }

        // Vertical matches
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight - 2; y++)
            {
                GameObject candy = grid[x, y];
                if (candy == null) continue;

                Sweet candyScript = candy.GetComponent<Sweet>();

                // Check for a vertical match
                if (grid[x, y + 1]?.GetComponent<Sweet>().originalSweet == candyScript.originalSweet &&
                    grid[x, y + 2]?.GetComponent<Sweet>().originalSweet == candyScript.originalSweet)
                {
                    // Add candies in the match to the HashSet
                    candiesToDestroy.Add(grid[x, y]);
                    candiesToDestroy.Add(grid[x, y + 1]);
                    candiesToDestroy.Add(grid[x, y + 2]);
                }
            }
        }

        foreach (GameObject candy in candiesToDestroy)
        {
            if (candy is not null)
            {
                Sweet candyScript = candy.GetComponent<Sweet>();
                if (candyScript is not null)
                    grid[candyScript.GridX, candyScript.GridY] = null;

                Destroy(candy);
            }
        }

        // Optionally: Call a function to refill the grid after candies are destroyed
        // RefillGrid();
    }
}
