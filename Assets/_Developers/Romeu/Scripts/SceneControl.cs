using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum MissionType { FastResponse, Pursuit, Rescue, Boss }

public class SceneControl : Singleton<SceneControl>
{
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private string gameplayScene = "Gameplay"; // carrega junto ao bioma
    [SerializeField] private string[] biomeScenes = new string[] { "BiomeCorrupted", "BiomeDesert", "BiomeFlorest", "BiomeMix", "BiomePlain", "BiomeOcean"  };

    [field:SerializeField] public MissionType currentMission { get; private set; }

    // audios com vozes dos personagens
    [field: SerializeField] public AudioClip[] FastResponseClips { get; private set; }
    [field: SerializeField] public AudioClip[] PursuitClips { get; private set; }
    [field: SerializeField] public AudioClip[] RescueClips { get; private set; }
    [field: SerializeField] public AudioClip[] BossClips { get; private set; }
    [field: SerializeField] public AudioClip[] PlayerResponseClips { get; private set; }

    public GameplayManager gameplayManager { get; private set; }

    public void OnEnable()
    {
        if (loadingCanvas == null)
            loadingCanvas = transform.GetComponentInChildren<Canvas>();
    }

    public Coroutine ChangeScene(string sceneName)
    {
        //if(loadingCanvas != null) loadingCanvas.gameObject.SetActive(true);
        return StartCoroutine(SceneChanging(sceneName));
    }

    IEnumerator SceneChanging(string sceneName, bool additive = false)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, (additive) ? LoadSceneMode.Additive : LoadSceneMode.Single);
        
        loadOp.allowSceneActivation = false;

        while (!loadOp.isDone)
        {
            if(loadOp.progress >= 0.9f)
            {
                loadOp.allowSceneActivation = true;
            }
            yield return null;
        }
        
        //if(loadingCanvas != null) loadingCanvas.gameObject.SetActive(false);
    }

    public Coroutine LoadBiomeScene(MissionType missionType)
    {
        currentMission = missionType;
        //if (loadingCanvas != null) loadingCanvas.gameObject.SetActive(true);
        return StartCoroutine(LoadBiomes());
    }

    IEnumerator LoadBiomes()
    {
        yield return null;
        string rndBiome = biomeScenes[Random.Range(0, biomeScenes.Length)];

        yield return StartCoroutine(SceneChanging(rndBiome));

        yield return StartCoroutine(SceneChanging(gameplayScene, true));

        //if (loadingCanvas != null) loadingCanvas.gameObject.SetActive(false);

        if (gameplayManager != null)
        {
            SceneControl.instance.ToggleLoading(false);
            gameplayManager.StartGameplay();
        }
    }

    public void ToggleLoading(bool visible)
    {
        if (loadingCanvas != null) loadingCanvas.gameObject.SetActive(visible);
    }

    public void AddGameplayManager(GameplayManager manager)
    {
        gameplayManager = manager;
    }
}
