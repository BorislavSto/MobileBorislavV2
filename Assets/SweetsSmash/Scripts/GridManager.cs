using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct BlockoutArea
{
    public Vector2 blockMin;
    public Vector2 blockMax;
}

public class GridManager : MonoBehaviour
{
    [Header("Configurations")] 
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float gridSpacing = 0.6f;
    public GameObject candyPrefab;
    public Transform gridParent;
    public SpriteRenderer backGround;

    [Header("Blockout Area")] 
    public BlockoutArea[] blockoutAreas;
    
    private GameManager gameManager;
    
    private bool isRefilling = false;
    private bool isProcessingMatches;
    private Vector2Int[] directions =
        { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };

    [HideInInspector] public GameObject[,] grid;

    public void StartGrid(int width, int height, BlockoutArea[] blockout, Sprite image)
    {
        gameManager = GameManager.Instance;
        
        grid = new GameObject[width, height];
        blockoutAreas = blockout;
        backGround.sprite = image;
        StartCoroutine(GenerateGrid());
    }

    // private void Start()
    // {
    //     grid = new GameObject[gridWidth, gridHeight];
    //     StartCoroutine(GenerateGrid());
    // }

    IEnumerator GenerateGrid()
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
                GameObject newCandy;
                bool hasMatch;
                do
                {
                    hasMatch = false;

                    Vector2 startPosition = targetposition + Vector2.up * 5f;

                    newCandy = Instantiate(candyPrefab, startPosition, Quaternion.identity, gridParent);

                    Sweet sweet = newCandy.GetComponent<Sweet>();
                    sweet.SetPosition(x, y);
                    sweet.SetType(UnityEngine.Random.Range(0, sweet.sweets.Length));
                    grid[x, y] = newCandy;

                    List<Vector2Int> matchList = GetConnectedCandies(x, y);
                    if (matchList.Count >= 3)
                    {
                        hasMatch = true;
                        Destroy(newCandy);
                    }
                } while (hasMatch);

                columnCandies.Add(newCandy);
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
                columnCandies[i].transform.position = targetPositions[i];
        }
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
                    grid[neighbor.x, neighbor.y] != null && // Ensure the neighbor exists
                    grid[start.x, start.y] != null && // Ensure the starting candy exists
                    grid[neighbor.x, neighbor.y].GetComponent<Sweet>().SweetID ==
                    grid[start.x, start.y].GetComponent<Sweet>().SweetID)
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
        if (!gameManager)
            gameManager = GameManager.Instance;
        
        if (gameManager.gameOver)
            return;
        
        if (isRefilling)
            return;
        
        HashSet<GameObject> candiesToDestroy = new HashSet<GameObject>();
        Dictionary<int, int> matchCounts = new Dictionary<int, int>(); // To track counts of matches by size for points
        bool candyWasDestroyed = false;

        // Horizontal matches
        for (int y = 0; y < gridHeight; y++)
        {
            int matchCount = 1; // Start with 1 candy in the match

            for (int x = 0; x < gridWidth; x++)
            {
                if (x == gridWidth - 1 || grid[x, y] == null || grid[x + 1, y] == null)
                {
                    if (matchCount >= 3)
                    {
                        // Log horizontal match
                        for (int i = 0; i < matchCount; i++)
                        {
                            candiesToDestroy.Add(grid[x - i, y]);
                        }

                        if (!matchCounts.ContainsKey(matchCount))
                            matchCounts[matchCount] = 0;
                        matchCounts[matchCount]++;
                    }

                    matchCount = 1; // Reset match count for next sequence
                    continue;
                }

                Sweet current = grid[x, y].GetComponent<Sweet>();
                Sweet next = grid[x + 1, y].GetComponent<Sweet>();

                if (current != null && next != null && current.SweetID == next.SweetID)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        // Log horizontal match
                        for (int i = 0; i < matchCount; i++)
                        {
                            candiesToDestroy.Add(grid[x - i, y]);
                        }

                        if (!matchCounts.ContainsKey(matchCount))
                            matchCounts[matchCount] = 0;
                        matchCounts[matchCount]++;
                    }

                    matchCount = 1; // Reset match count for next sequence
                }
            }
        }

        // Vertical matches
        for (int x = 0; x < gridWidth; x++)
        {
            int matchCount = 1; // Start with 1 candy in the match

            for (int y = 0; y < gridHeight; y++)
            {
                if (y == gridHeight - 1 || grid[x, y] == null || grid[x, y + 1] == null)
                {
                    if (matchCount >= 3)
                    {
                        // Log vertical match
                        for (int i = 0; i < matchCount; i++)
                        {
                            candiesToDestroy.Add(grid[x, y - i]);
                        }

                        if (!matchCounts.ContainsKey(matchCount))
                            matchCounts[matchCount] = 0;
                        matchCounts[matchCount]++;
                    }

                    matchCount = 1; // Reset match count for next sequence
                    continue;
                }

                Sweet current = grid[x, y].GetComponent<Sweet>();
                Sweet next = grid[x, y + 1].GetComponent<Sweet>();

                if (current != null && next != null && current.SweetID == next.SweetID)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        // Log vertical match
                        for (int i = 0; i < matchCount; i++)
                        {
                            candiesToDestroy.Add(grid[x, y - i]);
                        }

                        if (!matchCounts.ContainsKey(matchCount))
                            matchCounts[matchCount] = 0;
                        matchCounts[matchCount]++;
                    }

                    matchCount = 1; // Reset match count for next sequence
                }
            }
        }
    
        if (candiesToDestroy.Count > 0) 
            candyWasDestroyed = true;

        // Destroy matched candies
        foreach (GameObject candy in candiesToDestroy)
        {
            if (candy != null)
            {
                Sweet candyScript = candy.GetComponent<Sweet>();

                if (candyScript != null)
                    grid[candyScript.GridX, candyScript.GridY] = null;

                Destroy(candy);
            }
        }

        // Log match counts
        foreach (KeyValuePair<int, int> count in matchCounts)
        {
            int totalPoints = 0;
            int matchSize = count.Key;
            int matchCount = count.Value;
            int pointsForThisMatchSize = matchSize * 10;
            totalPoints += pointsForThisMatchSize * matchCount;
            
            gameManager.OnPointsGainedTrigger(totalPoints);

            Debug.Log($"Number of {count.Key}-candy matches: {count.Value}");
        }

        if (candyWasDestroyed) 
            RefillGrid();
    }


    public void RefillGrid()
    {
        if (!isRefilling)
        {
            isRefilling = true;
            StartCoroutine(RefillGridCoroutine());
        }
    }

    IEnumerator RefillGridCoroutine()
    { 
        Vector2 parentPosition = gridParent.position;

        for (int x = 0; x < gridWidth; x++)
        {
            int[] emptySpacesBelow = new int[gridHeight];
            int runningEmptyCount = 0;

            // Bottom-up pass to count empty spaces, accounting for blocked areas
            for (int y = 0; y < gridHeight; y++)
            {
                if (IsWithinBlockoutArea(x, y))
                    runningEmptyCount = 0;
                else if (grid[x, y] == null)
                    runningEmptyCount++;

                emptySpacesBelow[y] = runningEmptyCount;
            }

            // Create lists to store candies and their target positions
            List<GameObject> candiesToDrop = new List<GameObject>();
            List<Vector2> targetPositions = new List<Vector2>();
            List<Vector2Int> newGridPositions = new List<Vector2Int>();
            List<Vector2> startPositions = new List<Vector2>();

            for (int y = 0; y < gridHeight; y++)
            {
                if (IsWithinBlockoutArea(x, y))
                    continue;

                if (grid[x, y] != null && emptySpacesBelow[y] > 0)
                {
                    int newY = y - emptySpacesBelow[y];

                    if (newY >= 0 && !IsWithinBlockoutArea(x, newY))
                    {
                        candiesToDrop.Add(grid[x, y]);
                        startPositions.Add(grid[x, y].transform.position);
                        Vector2 targetPos = parentPosition + new Vector2(x * gridSpacing, newY * gridSpacing);
                        targetPositions.Add(targetPos);
                        newGridPositions.Add(new Vector2Int(x, newY));

                        grid[x, newY] = grid[x, y];
                        grid[x, y] = null;
                    }
                }
            }

            // Animate existing candies falling with cascade effect
            if (candiesToDrop.Count > 0)
            {
                yield return AnimateCascadingCandyFall(candiesToDrop, startPositions, targetPositions,
                    newGridPositions);
            }

            // Fill empty spaces from top, with cascade effect
            List<(GameObject candy, Vector2 start, Vector2 target, int gridX, int gridY)> newCandies =
                new List<(GameObject, Vector2, Vector2, int, int)>();

            for (int y = 0; y < gridHeight; y++)
            {
                if (IsWithinBlockoutArea(x, y) || grid[x, y] != null)
                    continue;

                if (grid[x, y] == null && !IsWithinBlockoutArea(x, y))
                {
                    Vector2 targetPosition = parentPosition + new Vector2(x * gridSpacing, y * gridSpacing);
                    Vector2 startPosition = targetPosition + Vector2.up * 5f;

                    GameObject newCandy = Instantiate(candyPrefab, startPosition, Quaternion.identity, gridParent);
                    Sweet sweet = newCandy.GetComponent<Sweet>();
                    grid[x, y] = newCandy;
                    sweet.SetPosition(x, y);
                    sweet.SetType(UnityEngine.Random.Range(0, 3));

                    newCandies.Add((newCandy, startPosition, targetPosition, x, y));
                }
            }

            // Animate new candies with cascade effect
            if (newCandies.Count > 0)
                yield return AnimateCascadingNewCandies(newCandies);

            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);

        isRefilling = false;

        // added as a preventative for candies falling during checks, breaking the game
        if (!isProcessingMatches)
            CheckForMatches();
    }

    private IEnumerator AnimateCascadingCandyFall(
        List<GameObject> candies,
        List<Vector2> startPositions,
        List<Vector2> targets,
        List<Vector2Int> gridPositions)
    {
        isProcessingMatches = true;
        float fallDuration = 0.3f;
        float cascadeDelay = 0.05f;

        for (int i = 0; i < candies.Count; i++)
        {
            if (candies[i] != null)
            {
                Vector2Int pos = gridPositions[i];
                if (!IsWithinBlockoutArea(pos.x, pos.y))
                {
                    Sweet sweet = candies[i].GetComponent<Sweet>();
                    sweet.SetPosition(pos.x, pos.y);
                }
            }
        }

        var sortedIndices = Enumerable.Range(0, candies.Count)
            .OrderBy(i => targets[i].y)
            .ToList();

        float[] startTimes = new float[candies.Count];
        float currentTime = 0f;

        while (currentTime < fallDuration + (cascadeDelay * candies.Count))
        {
            currentTime += Time.deltaTime;

            for (int idx = 0; idx < sortedIndices.Count; idx++)
            {
                int i = sortedIndices[idx];

                if (candies[i] == null) continue;

                if (currentTime >= idx * cascadeDelay)
                {
                    float candyTime = Mathf.Clamp01((currentTime - (idx * cascadeDelay)) / fallDuration);
                    candies[i].transform.position = Vector2.Lerp(startPositions[i], targets[i], candyTime);
                }
            }

            yield return null;
        }

        // Ensure all candies are in final positions
        for (int i = 0; i < candies.Count; i++)
        {
            if (candies[i] != null)
                candies[i].transform.position = targets[i];
        }

        isProcessingMatches = false;
    }

    private IEnumerator AnimateCascadingNewCandies(
        List<(GameObject candy, Vector2 start, Vector2 target, int gridX, int gridY)> newCandies)
    {
        isProcessingMatches = true;

        float fallDuration = 0.3f;
        float cascadeDelay = 0.05f;

        var sortedCandies = newCandies.OrderBy(c => c.target.y).ToList();

        float currentTime = 0f;
        while (currentTime < fallDuration + (cascadeDelay * sortedCandies.Count))
        {
            currentTime += Time.deltaTime;

            for (int i = 0; i < sortedCandies.Count; i++)
            {
                var candyData = sortedCandies[i];

                if (candyData.candy == null)
                    continue;

                float candyTime = Mathf.Clamp01((currentTime - (i * cascadeDelay)) / fallDuration);
                candyData.candy.transform.position = Vector2.Lerp(candyData.start, candyData.target, candyTime);
            }

            yield return null;
        }

        // Final position check
        foreach (var candyData in sortedCandies)
        {
            if (candyData.candy != null)
                candyData.candy.transform.position = candyData.target;
        }

        isProcessingMatches = false;
    }
}