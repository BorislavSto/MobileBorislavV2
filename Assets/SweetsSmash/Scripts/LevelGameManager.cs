using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelGameManager : MonoBehaviour
{
    public GridManager gridManager;
    public LevelDataScripatbleObject currentLevelData;
    public TMP_Text levelText;
    
    public int currentLevelNumber { get; private set; }
    
    public void SetupGame(LevelDataScripatbleObject levelData)
    {
        currentLevelData = levelData;
        gridManager.StartGrid(levelData.GridX, levelData.GridY, levelData.blockoutAreas, levelData.sprite);
        currentLevelNumber = levelData.levelNumber;
        levelText.text = levelData.levelName;
    }
}
