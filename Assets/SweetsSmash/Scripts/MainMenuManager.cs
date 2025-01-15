using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private SceneManagerController sceneManagerController;
    private GameManager gameManager;

    public GameObject mainMenu;
    public GameObject levelMenu;

    public List<Button> levelButtons;
    private List<int> totalLevels = new List<int>();
    private List<int> beatenLevels = new List<int>();

    public void Start()
    {
        sceneManagerController = SceneManagerController.Instance;
        gameManager = GameManager.Instance;

        beatenLevels = gameManager.beatenLevels;
        SetLevels();
    }

    private void SetLevels()
    {
        foreach (int level in totalLevels)
        {
            if (beatenLevels.Contains(level))
                levelButtons[level].interactable = true;
        }
    }

    public void OpenGameMap()
    {
        mainMenu.SetActive(false);
        levelMenu.SetActive(true);
    }

    public void CloseGameMap()
    {
        levelMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void PlayLevel(LevelDataScripatbleObject levelDataScripatbleObject)
    {
        sceneManagerController.LoadLevelFromMenu(levelDataScripatbleObject);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
