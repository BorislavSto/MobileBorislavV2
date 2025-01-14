using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGameManager : MonoBehaviour
{
    public GridManager gridManager;
    
    public void SetupGame(LevelDataScripatbleObject levelData)
    {
        gridManager.StartGrid(levelData.GridX, levelData.GridY, levelData.blockoutAreas, levelData.sprite);
    }
}
