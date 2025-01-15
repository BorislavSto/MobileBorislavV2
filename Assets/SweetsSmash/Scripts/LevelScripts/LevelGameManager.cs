using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelGameManager : MonoBehaviour
{
    public GridManager gridManager;
    public LevelDataScripatbleObject currentLevelData;
    public TMP_Text levelText;
    public TMP_Text LevelTurnsToWinText;
    public TMP_Text LevelTurnsTakenText;
    public TMP_Text LevelPointsNeededToWinText;
    public TMP_Text LevelPointsWonText;
    
    private int turnsTaken = 0;
    private int pointsWon = 0;
    public int turnsToWin = 0;
    public int pointsToWin = 0;
    
    private GameManager gameManager;
    
    
    public int currentLevelNumber { get; private set; }
    
    public void SetupGame(LevelDataScripatbleObject levelData)
    {
        gameManager = GameManager.Instance;
        gameManager.OnTurnTaken += TurnTaken;
        gameManager.OnPointsGained += UpdateScore;
        gameManager.gameOver = false;

        currentLevelData = levelData;
        gridManager.StartGrid(levelData.GridX, levelData.GridY, levelData.blockoutAreas, levelData.sprite);
        currentLevelNumber = levelData.levelNumber;
        levelText.text = levelData.levelName;
        
        turnsToWin = levelData.turnsToWin;
        pointsToWin = levelData.pointsToWin;
        LevelTurnsToWinText.text = turnsToWin.ToString();
        LevelPointsNeededToWinText.text = pointsToWin.ToString();
        
        LevelTurnsTakenText.text = "0";
        LevelPointsWonText.text = "0";
        turnsTaken = 0;
        pointsWon = 0;
    }

    private void OnDestroy()
    {
        gameManager.OnTurnTaken -= TurnTaken;
    }

    public void UpdateScore(int score)
    {
        pointsWon += score;
        LevelPointsWonText.text = pointsWon.ToString();

        if (pointsWon >= pointsToWin)
        {
            gameManager.OnGameEndTrigger(true);
            gameManager.gameOver = true;
        }
    }

    public void TurnTaken()
    {
        turnsTaken++;
        LevelTurnsTakenText.text = turnsTaken.ToString();

        if (turnsTaken >= turnsToWin)
        {
            gameManager.OnGameEndTrigger(false);
            gameManager.gameOver = true;
        }
    }
}
