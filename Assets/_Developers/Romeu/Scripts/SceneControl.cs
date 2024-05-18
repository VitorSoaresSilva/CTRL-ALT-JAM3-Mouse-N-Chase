using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum MissionType { FastResponse, Pursuit, Rescue, Boss }

public class SceneControl : Singleton<SceneControl>
{
    [SerializeField] private string gameplayScene = "Gameplay"; // carrega junto ao bioma
    [SerializeField] private string[] biomeScenes = new string[] { "BiomeCorrupted", "BiomeDesert", "BiomeFlorest", "BiomeMix" };

    public MissionType currentMission { get; private set; }
    public GameplayManager gameplayManager { get; private set; }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void LoadBiomeScene(MissionType missionType)
    {
        currentMission = missionType;
        StartCoroutine(LoadBiomes());
    }

    IEnumerator LoadBiomes()
    {
        AsyncOperation biomeLoad = SceneManager.LoadSceneAsync(biomeScenes[Random.Range(0, biomeScenes.Length)]);
        biomeLoad.allowSceneActivation = false;

        while (!biomeLoad.isDone)
        {
            if (biomeLoad.progress >= 0.9f)
            {
                biomeLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        AsyncOperation gameLoad = SceneManager.LoadSceneAsync(gameplayScene, LoadSceneMode.Additive);
        gameLoad.allowSceneActivation = false;

        while (!gameLoad.isDone)
        {
            if (gameLoad.progress >= 0.9f)
            {
                gameLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        if(gameplayManager != null)
        {
            gameplayManager.StartGameplay();
        }
    }

    public void AddGameplayManager(GameplayManager manager)
    {
        gameplayManager = manager;
    }
}
