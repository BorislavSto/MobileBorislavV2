using UnityEngine;
using Lean.Touch;

public class CandySelectionManager : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject selectedCandy;

    void OnEnable()
    {
        LeanTouch.OnFingerTap += HandleTap;
        LeanTouch.OnFingerSwipe += HandleSwipe;
        Debug.Log("Candy Selection Manager enabled");
    }

    void OnDisable()
    {
        LeanTouch.OnFingerTap -= HandleTap;
        LeanTouch.OnFingerSwipe -= HandleSwipe;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void HandleTap(LeanFinger finger)
    {
        Debug.Log("Touch detected");

        Vector3 worldPosition = finger.GetWorldPosition(10f);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider)
        {
            GameObject candy = hit.collider.gameObject;

            if (selectedCandy != candy)
            {
                SelectCandy(candy);
            }
            else
            {
                DeselectCandy();
            }
        }
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    void HandleSwipe(LeanFinger finger)
    {
        if (selectedCandy is null)
            return;

        // Convert finger start & end positions to world space
        Vector3 start = finger.GetStartWorldPosition(10f);
        Vector3 end = finger.GetWorldPosition(10f);

        // Calculate swipe direction
        Vector3 direction = end - start;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal swipe
            if (direction.x > 0)
                SwapCandy(selectedCandy, Vector2.right); // Swipe right
            else
                SwapCandy(selectedCandy, Vector2.left);  // Swipe left
        }
        else
        {
            // Vertical swipe
            if (direction.y > 0)
                SwapCandy(selectedCandy, Vector2.up);    // Swipe up
            else
                SwapCandy(selectedCandy, Vector2.down);  // Swipe down
        }
    }

    void SwapCandy(GameObject candy, Vector2 direction)
    {
        if (candy is null) return;
        
        Sweet candyScript = candy.GetComponent<Sweet>();
        if (candyScript is null) return;

        int targetX = candyScript.GridX + (int)direction.x;
        int targetY = candyScript.GridY + (int)direction.y;

        // Check bounds
        if (targetX < 0 || targetX >= gridManager.gridWidth || targetY < 0 || targetY >= gridManager.gridHeight)
            return;

        GameObject targetCandy = gridManager.grid[targetX, targetY];
        if (targetCandy == null) return;

        gridManager.grid[candyScript.GridX, candyScript.GridY] = targetCandy;
        gridManager.grid[targetX, targetY] = candy;

        (targetCandy.transform.position, candy.transform.position) = (candy.transform.position, targetCandy.transform.position);

        candy.GetComponent<Sweet>().SetPosition(targetX, targetY);
        targetCandy.GetComponent<Sweet>().SetPosition(candyScript.GridX, candyScript.GridY);

        DeselectCandy();

        gridManager.CheckForMatches();
    }


    void SelectCandy(GameObject candy)
    {
        if (selectedCandy)
            DeselectCandy();

        selectedCandy = candy;
        HighlightCandy(selectedCandy, true);

        Sweet sweetComponent = selectedCandy.GetComponent<Sweet>();
        if (sweetComponent != null)
            Debug.Log($"Selected Candy: {sweetComponent.name}");
    }

    void DeselectCandy()
    {
        if (selectedCandy)
        {
            HighlightCandy(selectedCandy, false);
            selectedCandy = null;
        }
    }

    void HighlightCandy(GameObject candy, bool highlight)
    {
        var spriteRenderer = candy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (highlight)
            {
                spriteRenderer.color = Color.white;
            }
            else
            {
                Sweet sweetComponent = candy.GetComponent<Sweet>();
                if (sweetComponent != null)
                    spriteRenderer.color = sweetComponent.GetOriginalColor();
            }
        }
    }
}