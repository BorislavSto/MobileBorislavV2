using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

public enum Scenes
{
    Loading,
    Options,
    MainMenu,
    Levels,
    SharedSystems,
}

public class SceneManagerController : MonoBehaviour
{
    public static SceneManagerController Instance { get; private set; }
    public List<Scenes> ActiveScenes = new();
    
    public event Action<Scenes> OnSceneChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitLoad();
    }

    public void LoadScene(Scenes scene)
    {
        StartCoroutine(LoadSceneAsync(scene));
    }

    #region InitialLoad
    private void InitLoad()
    {
        StartCoroutine(LoadSetup());
    }

    private IEnumerator LoadSetup()
    {
        yield return SceneManager.LoadSceneAsync(Scenes.Loading.ToString(), LoadSceneMode.Additive);

        List<Scenes> scenesToLoad = new List<Scenes>
        {
            Scenes.SharedSystems,
            Scenes.Options,
            Scenes.MainMenu
        };

        List<AsyncOperation> loadOperations = new List<AsyncOperation>();
        foreach (Scenes scene in scenesToLoad)
        {
            loadOperations.Add(SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive));
        }

        foreach (AsyncOperation operation in loadOperations)
        {
            yield return new WaitUntil(() => operation.isDone);
        }

        yield return SceneManager.UnloadSceneAsync(Scenes.Loading.ToString());

        foreach (Scenes scene in scenesToLoad)
        {
            ActiveScenes.Add(scene);
            OnSceneChanged?.Invoke(scene);
        }
    } 
    #endregion
    
    private IEnumerator LoadSceneAsync(Scenes scene)
    {
        SceneManager.LoadScene(Scenes.Loading.ToString(), LoadSceneMode.Additive);
        yield return null;

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive);
        while (!asyncOp.isDone)
            yield return null;
        
        if (asyncOp.isDone)
                UnloadScene(Scenes.Loading);
        
        ActiveScenes.Add(scene);
        OnSceneChanged?.Invoke(scene);
    }

    public void UnloadScene(Scenes scene)
    {
        Scene unloadedScene = SceneManager.GetSceneByName(scene.ToString());
        if (unloadedScene.IsValid())
        {
            SceneManager.UnloadSceneAsync(scene.ToString());
            System.GC.Collect();

            if (ActiveScenes.Contains(scene))
                ActiveScenes.Remove(scene);
        }
        else
            Debug.LogError($"Failed to load scene: {scene}");
    }

    public void LoadLevel(LevelDataScripatbleObject levelData)
    {
        UnloadScene(Scenes.MainMenu);

        SceneManager.LoadSceneAsync(Scenes.Levels.ToString(), LoadSceneMode.Additive)
            .completed += (operation) =>
        {
            LevelGameManager levelObject = FindObjectOfType<LevelGameManager>();

            if (levelObject != null)
            {
                levelObject.SetupGame(levelData);
                ActiveScenes.Add(Scenes.Levels);
            }
            else
                Debug.LogError("LevelController object not found in the scene!");
        };
    }
    
    public void LoadAllSystemsScene()
    {
        SceneManager.LoadScene(Scenes.SharedSystems.ToString(), LoadSceneMode.Additive);
        ActiveScenes.Add(Scenes.SharedSystems);
    }   
    
    public void LoadOptionsScene()
    {
        SceneManager.LoadScene(Scenes.Options.ToString(), LoadSceneMode.Additive);
        ActiveScenes.Add(Scenes.Options);
    }
}
