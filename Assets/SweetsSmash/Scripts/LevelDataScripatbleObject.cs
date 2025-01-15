using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "ScriptableObjects/NewLevelData", order = 1)]
public class LevelDataScripatbleObject : ScriptableObject
{
    [Header("Generator")] 
    public int GridX;
    public int GridY;
    public BlockoutArea[] blockoutAreas;
    
    [Header("Other Data?")]
    public string levelName;
    public int levelNumber;
    public Sprite sprite;
}
