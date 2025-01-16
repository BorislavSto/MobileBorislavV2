using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int lives = 0;
    public List<int> beatenLevels = new List<int>(); //{ get; private set; }
    
    public bool gameOver = false;
    
    [Tooltip("If true won, if false lost")]
    public event Action<bool> OnGameEnd;
    public event Action<int> OnPointsGained;
    public event Action OnTurnTaken;
    public event Action<int> OnLivesChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnSucceededLevel(int level)
    {
        beatenLevels.Add(level);
        SaveData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("PlayerLives", lives);
        string levelsString = string.Join(",", beatenLevels); // Convert list to a comma-separated string
        PlayerPrefs.SetString("BeatenLevels", levelsString);
    }
    
    private void LoadBeatenLevels()
    {
        string levelsString = PlayerPrefs.GetString("BeatenLevels", ""); // Get the saved string
        if (!string.IsNullOrEmpty(levelsString))
        {
            // Convert the string back into a list of integers
            beatenLevels = new List<int>(Array.ConvertAll(levelsString.Split(','), int.Parse));
        }
        else
        {
            beatenLevels = new List<int>(); // Default if no saved data exists
        }
    }

    public void OnGameEndTrigger(bool didWin) { OnGameEnd?.Invoke(didWin); }
    public void OnTurnTakenTrigger() { OnTurnTaken?.Invoke(); }
    public void OnPointsGainedTrigger(int score) { OnPointsGained?.Invoke(score); }

    public void OnLivesChangedTrigger(int changeToLives)
    {
        lives += changeToLives;
        OnLivesChanged?.Invoke(lives);
    }
}
