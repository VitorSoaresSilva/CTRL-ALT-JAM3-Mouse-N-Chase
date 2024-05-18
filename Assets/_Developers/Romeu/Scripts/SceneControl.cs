using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : Singleton<SceneControl>
{
    [SerializeField] private string gameplayScene = "CarPathFollow"; // carrega junto ao bioma
    [SerializeField] private string[] biomeScenes = new string[] { "BiomeCorrupted", "BiomeDesert", "BiomeFlorest", "BiomeMix" };

    public AsyncOperation ChangeScene(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName);
    }

    public AsyncOperation LoadBiomeScene()
    {
        SceneManager.LoadSceneAsync(biomeScenes[Random.Range(0, biomeScenes.Length)]);
        return SceneManager.LoadSceneAsync(gameplayScene, LoadSceneMode.Additive);
    }
}
