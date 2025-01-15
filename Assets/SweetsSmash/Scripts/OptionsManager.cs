using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [Header("Options")]
    public GameObject optionsPanel;
    public GameObject settingsPanel;
    
    [Header("BigBackButton")] 
    public Button closeOptionsButtonBig;

    [Header("Buttons")] 
    public Button openOptionsButton;
    public Button closeOptionsButton;
    public Button backToOptions;
    public Button settingsButton;
    public Button mainMenuButton;
    
    [Header("Sliders")]
    public Slider volumeSlider;
    
    [Header("LevelMenus")]
    public GameObject adWindow;
    public GameObject winMenu;
    public GameObject loseMenu;
    
    private SceneManagerController sceneManagerController;
    public GameManager gameManager;
    private AudioMixer audioMixer;

    private void Start()
    {
        sceneManagerController = SceneManagerController.Instance;
        gameManager = GameManager.Instance;
        audioMixer = FindObjectOfType<AudioMixer>();

        gameManager.OnGameEnd += OnGameEnd;
    }

    public void OnOpenOptionsButtonClicked()
    {
        if (sceneManagerController.ActiveScenes.Contains(Scenes.MainMenu))
            mainMenuButton.gameObject.SetActive(false);
        
        optionsPanel.SetActive(true);
        closeOptionsButtonBig.gameObject.SetActive(true);
        openOptionsButton.gameObject.SetActive(false);
    }

    public void OnCloseOptionsButtonClicked()
    {
        if (settingsPanel.activeSelf)
            settingsPanel.SetActive(false);
            
        optionsPanel.SetActive(false);
        closeOptionsButtonBig.gameObject.SetActive(false);
        openOptionsButton.gameObject.SetActive(true);
    }

    public void OnSettingsButtonClicked()
    {
        settingsPanel.SetActive(true);
    }

    public void OnMainMenuButtonClicked()
    {
        sceneManagerController.UnloadScene(Scenes.Levels);
        sceneManagerController.LoadScene(Scenes.MainMenu);

        loseMenu.SetActive(false);

        OnCloseOptionsButtonClicked();
    }

    public void OnBackToOptionsButtonClicked()
    {
        settingsPanel.SetActive(false);
    }

    public void OnVolumeSliderChanged(float sliderValue)
    {
        float volume = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat("Volume", volume);
    }

    private void OnGameEnd(bool didWin)
    {
        if (didWin)
            ShowWinMenu();
        else
            ShowLoseMenu();
    }
    
    public void ShowWinMenu()
    {
        winMenu.SetActive(true);
    }   
    
    public void ShowLoseMenu()
    {
        loseMenu.SetActive(true);
    }

    public void BackToMenuWinButton()
    {
        LevelGameManager levelObject = FindObjectOfType<LevelGameManager>();
        int levelNumber = levelObject.currentLevelNumber; 
        Scenes level = levelObject.currentLevelData.levelType;

        gameManager.OnSucceededLevel(levelNumber);
        sceneManagerController.UnloadScene(level);
        sceneManagerController.LoadScene(Scenes.MainMenu);
        
        winMenu.SetActive(false);
        OnCloseOptionsButtonClicked();
    }
    
    public void RetryButton()
    {
        if (gameManager.lives == 0)
        {
            adWindow.SetActive(true);
            return;
        }
        
        gameManager.lives--;
        
        LevelGameManager levelObject = FindObjectOfType<LevelGameManager>();
        LevelDataScripatbleObject levelData = levelObject.currentLevelData;
        
        loseMenu.SetActive(false);

        sceneManagerController.ReloadLevel(levelData);
    }
}
