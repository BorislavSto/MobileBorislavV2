using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

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
    
    private Vector2Int[] directions = { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };

    [Header("Blockout Area")] public BlockoutArea[] blockoutAreas;

    [HideInInspector] public GameObject[,] grid;

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        GenerateGrid();
        //StartCoroutine(InitializeGrid());
    }

    void GenerateGrid()
    {
        Stopwatch stopwatch = Stopwatch.StartNew(); // Start timing

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                bool hasMatch;
                do
                {
                    hasMatch = false;

                    Vector3 candyPosition = gridParent.position + new Vector3(x * gridSpacing, y * gridSpacing, 0f);
                    GameObject newCandy = Instantiate(candyPrefab, candyPosition, Quaternion.identity, gridParent);

                    Sweet sweet = newCandy.GetComponent<Sweet>();
                    sweet.SetPosition(x, y);
                    sweet.SetType(Random.Range(0, sweet.sweets.Length)); // Assuming candyTypes is the number of types available
                    grid[x, y] = newCandy;

                    // Check for matches in the Von Neumann neighborhood
                    List<Vector2Int> matchList = GetConnectedCandies(x, y);
                    if (matchList.Count >= 3)
                    {
                        hasMatch = true; // Re-roll if there's a match
                        Destroy(newCandy); // Clean up the invalid candy
                    }
                } while (hasMatch);
            }
        }

        stopwatch.Stop(); // Stop timing
        UnityEngine.Debug.Log($"Grid generation completed in {stopwatch.ElapsedMilliseconds} ms");
    }


    List<Vector2Int> GetConnectedCandies(int startX, int startY)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        List<Vector2Int> matchList = new List<Vector2Int>();
        bool[,] visited = new bool[gridWidth, gridHeight];
        Vector2Int start = new Vector2Int(startX, startY);
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            if (visited[current.x, current.y]) continue;

            visited[current.x, current.y] = true;
            matchList.Add(current);

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (IsInBounds(neighbor.x, neighbor.y) && 
                    !visited[neighbor.x, neighbor.y] && 
                    grid[neighbor.x, neighbor.y] != null &&  // Ensure the neighbor exists
                    grid[start.x, start.y] != null &&       // Ensure the starting candy exists
                    grid[neighbor.x, neighbor.y].GetComponent<Sweet>().originalSweetName ==
                    grid[start.x, start.y].GetComponent<Sweet>().originalSweetName)
                {
                    stack.Push(neighbor);
                }
            }
        }

        return matchList;
    }

    bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
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

                Debug.Log(
                    $"Checking horizontal match at ({x}, {y}): {current.originalSweetName}, {next1.originalSweetName}, {next2.originalSweetName}");

                if (current.originalSweetName == next1.originalSweetName &&
                    current.originalSweetName == next2.originalSweetName)
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

                if (current.originalSweetName == next1.originalSweetName &&
                    current.originalSweetName == next2.originalSweetName)
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
                if (grid[x, y] == null && !IsWithinBlockoutArea(x, y))
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0 && grid[x, y] != null)
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
                    Debug.Log($"candy number {i} = {targetPositions[i]}");
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
