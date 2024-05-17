using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : Singleton<SceneControl>
{
    public AsyncOperation ChangeScene(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName);
    }
}
