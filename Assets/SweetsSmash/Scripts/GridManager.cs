using System;
using System.Collections;
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

    [Header("Blockout Area")] public BlockoutArea[] blockoutAreas;

    [HideInInspector] public GameObject[,] grid;

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        StartCoroutine(InitializeGrid());
        //StartCoroutine(AnimateGridInitialization());
    }

    IEnumerator AnimateGridInitialization()
    {
        Vector2 parentPosition = gridParent.position;

        for (int x = 0; x < gridWidth; x++)
        {
            List<GameObject> candiesInColumn = new List<GameObject>();

            for (int y = gridHeight - 1; y >= 0; y--) // Top to bottom
            {
                if (IsWithinBlockoutArea(x, y))
                    continue;

                Vector2 targetPosition = parentPosition + new Vector2(x * gridSpacing, y * gridSpacing);
                Vector2 startPosition = targetPosition + Vector2.up * 5f; // Start candies above the grid

                GameObject candy = Instantiate(candyPrefab, startPosition, Quaternion.identity, gridParent);
                candiesInColumn.Add(candy);

                grid[x, y] = candy;
                candy.GetComponent<Sweet>().SetPosition(x, y);
            }

            // Animate all candies in this column
            StartCoroutine(AnimateColumnFall(candiesInColumn, x));

            // Slight delay before moving to the next column
            yield return new WaitForSeconds(0.1f); 
        }
    }

    IEnumerator AnimateColumnFall(List<GameObject> candies, int column)
    {
        float fallDuration = 0.3f; // Faster animation

        foreach (GameObject candy in candies)
        {
            if (candy)
            {
                Vector3 targetPosition = candy.transform.position - Vector3.up * 5f; // End at grid position
                Vector3 startPosition = candy.transform.position;

                float elapsedTime = 0f;

                while (elapsedTime < fallDuration)
                {
                    candy.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / fallDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                candy.transform.position = targetPosition; // Snap to position
            }
        }
    }

    IEnumerator InitializeGrid()
    {
        Vector2 parentPosition = gridParent.position;
        
        for (int y = 0; y < gridHeight; y++)
        {
            List<GameObject> columnCandies = new List<GameObject>();

            for (int x = 0; x < gridWidth; x++)
            {
                if (IsWithinBlockoutArea(x, y))
                    continue;

                Vector2 targetposition = parentPosition + new Vector2(x * gridSpacing, y * gridSpacing);
                Vector2 startPosition = targetposition + Vector2.up * 5f;
                GameObject candy = Instantiate(candyPrefab, startPosition, Quaternion.identity, gridParent);
                grid[x, y] = candy;
                candy.GetComponent<Sweet>().SetPosition(x, y);
                
                columnCandies.Add(candy);
                
                float fallDuration = 0.3f;
                float elapsedTime = 0f;

                while (elapsedTime < fallDuration)
                {
                    candy.transform.position = Vector3.Lerp(startPosition, targetposition, elapsedTime / fallDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
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
        HashSet<GameObject> candiesToDestroy = new HashSet<GameObject>();

        // Horizontal matches
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth - 2; x++)
            {
                if (grid[x, y] == null || grid[x + 1, y] == null || grid[x + 2, y] == null)
                    continue;

                Sweet current = grid[x, y].GetComponent<Sweet>();
                Sweet next1 = grid[x + 1, y].GetComponent<Sweet>();
                Sweet next2 = grid[x + 2, y].GetComponent<Sweet>();

                Debug.Log($"Checking horizontal match at ({x}, {y}): {current.originalSweetName}, {next1.originalSweetName}, {next2.originalSweetName}");

                if (current.originalSweetName == next1.originalSweetName && current.originalSweetName == next2.originalSweetName)
                {
                    Debug.Log($"Horizontal match found at ({x}, {y}) with type {current.originalSweetName}");
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
                if (grid[x, y] == null || grid[x, y + 1] == null || grid[x, y + 2] == null)
                    continue;

                Sweet current = grid[x, y].GetComponent<Sweet>();
                Sweet next1 = grid[x, y + 1].GetComponent<Sweet>();
                Sweet next2 = grid[x, y + 2].GetComponent<Sweet>();

                Debug.Log(
                    $"Checking vertical match at ({x}, {y}): {current.originalSweetName}, {next1.originalSweetName}, {next2.originalSweetName}");

                if (current.originalSweetName == next1.originalSweetName && current.originalSweetName == next2.originalSweetName)
                {
                    Debug.Log($"Vertical match found at ({x}, {y}) with type {current.originalSweetName}");
                    candiesToDestroy.Add(grid[x, y]);
                    candiesToDestroy.Add(grid[x, y + 1]);
                    candiesToDestroy.Add(grid[x, y + 2]);
                }
            }
        }

        // Destroy matched candies
        foreach (GameObject candy in candiesToDestroy)
        {
            if (candy != null)
            {
                Sweet candyScript = candy.GetComponent<Sweet>();
                Debug.Log(
                    $"Destroying candy at ({candyScript.GridX}, {candyScript.GridY}) with type {candyScript.originalSweetName}");

                if (candyScript != null)
                    grid[candyScript.GridX, candyScript.GridY] = null;

                Destroy(candy);
            }
        }
        // Optionally: Call a function to refill the grid after candies are destroyed
        // RefillGrid();
    }
}
