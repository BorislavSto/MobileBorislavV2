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
    Sound,
    Options,
    MainMenu,
    Levels,
}

public class SceneManagerController : MonoBehaviour
{
    public static SceneManagerController Instance { get; private set; }

    public Scenes CurrentScene { get; private set; }
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

        LoadScene(Scenes.MainMenu);
    }

    public void LoadScene(Scenes scene)
    {
        StartCoroutine(LoadSceneAsync(scene));
    }

    public void ReloadCurrentScene()
    {
        LoadScene(CurrentScene);
    }

    private IEnumerator LoadSceneAsync(Scenes scene)
    {
        SceneManager.LoadScene(Scenes.Loading.ToString(), LoadSceneMode.Additive);
        yield return null;

        // Load the target scene
        var asyncOp = SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive);
        while (!asyncOp.isDone)
        {
            // Optional: Handle progress (asyncOp.progress)
            yield return null;
        }
    
        if (asyncOp.isDone)
                UnloadScene(Scenes.Loading);
        
        CurrentScene = scene;
        OnSceneChanged?.Invoke(scene);
    }

    private void SaveScene(Scenes scene)
    {
        
    }

    public void UnloadScene(Scenes scene)
    {
        Scene unloadedScene = SceneManager.GetSceneByName(scene.ToString());
        if (unloadedScene.IsValid())
        {
            Debug.Log($"Successfully loaded scene: {unloadedScene.name}");
            SceneManager.UnloadSceneAsync(scene.ToString());
        }
        else
            Debug.LogError($"Failed to load scene: {scene}");
    }
    
    public void LoadSoundScene()
    {
        SceneManager.LoadScene(Scenes.Sound.ToString(), LoadSceneMode.Additive);
    }
    
    public void LoadOptionsScene()
    {
        SceneManager.LoadScene(Scenes.Sound.ToString(), LoadSceneMode.Additive);
    }
}
