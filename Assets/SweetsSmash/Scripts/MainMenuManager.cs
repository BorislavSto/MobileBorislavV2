using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private SceneManagerController sceneManagerController;


    public void Start()
    {
        sceneManagerController = SceneManagerController.Instance;
    }

    public void OpenGameMap()
    {
        
    }

    public void PlayLevel(LevelDataScripatbleObject levelDataScripatbleObject)
    {
        sceneManagerController.LoadLevel(levelDataScripatbleObject);
    }

    public void BackButton()
    {
        
    }
}
