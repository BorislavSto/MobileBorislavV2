using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int lives = 0;
    public List<int> beatenLevels { get; private set; }
    
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
}
