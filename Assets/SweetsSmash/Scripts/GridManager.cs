using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

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
    protected Vector2 SpawnHeight = new Vector2(0, 6);

    [Header("Blockout Area")] public BlockoutArea[] blockoutAreas;

    [HideInInspector] public GameObject[,] grid;

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        StartCoroutine(InitializeGrid());
    }

    IEnumerator InitializeGrid()
    {
        Vector2 parentPosition = gridParent.position;

        for (int y = 0; y < gridHeight; y++)
        {
            List<GameObject> columnCandies = new();
            List<Vector2> targetPositions = new();

            for (int x = 0; x < gridWidth; x++)
            {
                if (IsWithinBlockoutArea(x, y))
                    continue;

                Vector2 targetposition = parentPosition + new Vector2(x * gridSpacing, y * gridSpacing);
                Vector2 startPosition = targetposition + Vector2.up * 5f;

                GameObject candy = Instantiate(candyPrefab, startPosition, Quaternion.identity, gridParent);
                grid[x, y] = candy;
                candy.GetComponent<Sweet>().SetPosition(x, y);
                candy.GetComponent<Sweet>().SetType(UnityEngine.Random.Range(0, 3));

                columnCandies.Add(candy);
                targetPositions.Add(targetposition);
            }

            float fallDuration = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < fallDuration)
            {
                float t = elapsedTime / fallDuration;

                for (int i = 0; i < columnCandies.Count; i++)
                {
                    Vector2 startPosition = targetPositions[i] + Vector2.up * 5f;
                    columnCandies[i].transform.position = Vector3.Lerp(startPosition, targetPositions[i], t);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < columnCandies.Count; i++)
            {
                columnCandies[i].transform.position = targetPositions[i];
            }
        }
        CheckForMatches();
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

                Debug.Log($"Checking horizontal match at ({x}, {y}): {current.SweetID}, {next1.SweetID}, {next2.SweetID}");

                if (current.SweetID == next1.SweetID && current.SweetID == next2.SweetID)
                {
                    Debug.Log($"Horizontal match found at ({x}, {y}) with type {current.SweetID}");
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
                    $"Checking vertical match at ({x}, {y}): {current.SweetID}, {next1.SweetID}, {next2.SweetID}");

                if (current.SweetID == next1.SweetID && current.SweetID == next2.SweetID)
                {
                    Debug.Log($"Vertical match found at ({x}, {y}) with type {current.SweetID}");
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
                    $"Destroying candy at ({candyScript.GridX}, {candyScript.GridY}) with type {candyScript.SweetID}");

                if (candyScript != null)
                    grid[candyScript.GridX, candyScript.GridY] = null;

                Destroy(candy);
            }
        }
        RefillGrid();
    }

    public void RefillGrid()
    {
        StartCoroutine(RefillGridCoroutine());
    }

    IEnumerator RefillGridCoroutine()
    {
        Vector2 parentPosition = gridParent.position;

        for (int x = 0; x < gridWidth; x++)
        {
            List<GameObject> candiesToDrop = new List<GameObject>();
            List<Vector2> targetPositions = new List<Vector2>();
            int emptySpaces = 0;

            // Check each column from bottom to top
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    // Move candy down by the number of empty spaces
                    candiesToDrop.Add(grid[x, y]);
                    targetPositions.Add(parentPosition + new Vector2(x * gridSpacing, (y - emptySpaces) * gridSpacing));

                    // Update the grid
                    grid[x, y - emptySpaces] = grid[x, y];
                    grid[x, y] = null;
                }
            }

            // Animate existing candies moving down
            float fallDuration = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < fallDuration)
            {
                float t = elapsedTime / fallDuration;

                for (int i = 0; i < candiesToDrop.Count; i++)
                {
                    Vector2 startPosition = candiesToDrop[i].transform.position;
                    candiesToDrop[i].transform.position = Vector3.Lerp(startPosition, targetPositions[i], t);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < candiesToDrop.Count; i++)
            {
                candiesToDrop[i].transform.position = targetPositions[i];
                Sweet sweet = candiesToDrop[i].GetComponent<Sweet>();
                sweet.SetPosition(x, (int)(targetPositions[i].y / gridSpacing));
            }

            // Spawn new candies for empty spaces at the top
            for (int y = gridHeight - emptySpaces; y < gridHeight; y++)
            {
                Vector2 targetPosition = parentPosition + new Vector2(x * gridSpacing, y * gridSpacing);
                Vector2 startPosition = targetPosition + Vector2.up * 5f;

                GameObject newCandy = Instantiate(candyPrefab, startPosition, Quaternion.identity, gridParent);
                newCandy.GetComponent<Sweet>().SetPosition(x, y);
                grid[x, y] = newCandy;

                float spawnElapsedTime = 0f;

                while (spawnElapsedTime < fallDuration)
                {
                    float t = spawnElapsedTime / fallDuration;
                    newCandy.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                    spawnElapsedTime += Time.deltaTime;
                    yield return null;
                }

                newCandy.transform.position = targetPosition;
            }
        }

        // After refilling, check for new matches
        yield return new WaitForEndOfFrame();
        CheckForMatches();
    }
}
