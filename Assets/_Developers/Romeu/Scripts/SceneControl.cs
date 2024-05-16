using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : Singleton<SceneControl>
{
    [SerializeField] private RectTransform transitionCircle;
    [SerializeField] private Vector2 CirclePoint;
    [SerializeField] private float transitionSpeed = 1.0f;
    [SerializeField] private Vector2 minCircleSize = new(0, 0);
    [SerializeField] private Vector2 maxCircleSize = new(1920, 1920);

    private bool fading = false;

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(TransitionScene(sceneName));
    }

    IEnumerator TransitionScene(string sceneName)
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName);
        sceneLoad.allowSceneActivation = false;

        transitionCircle.gameObject.SetActive(true);
        FadeOut();

        while(!sceneLoad.isDone)
        {
            
            if (sceneLoad.progress >= 0.9f)
                sceneLoad.allowSceneActivation = true;

            yield return null;
        }

        FadeIn();
        transitionCircle.gameObject.SetActive(false);
    }

    public void FadeIn() // Deixa a tela preta, sem o círculo
    {
        //StartCoroutine(FadeCircle(minCircleSize, maxCircleSize));
        ScaleOverTime(minCircleSize, maxCircleSize, transitionSpeed);
    }

    public void FadeOut() // Abre a tela preta, com o círculo
    {
        //StartCoroutine(FadeCircle(maxCircleSize, minCircleSize));
        ScaleOverTime(maxCircleSize, minCircleSize, transitionSpeed);
    }

    // unificar com carregamento da cena
    private IEnumerator ScaleOverTime(Vector2 initialScale, Vector2 targetSize, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transitionCircle.localScale = Vector2.Lerp(initialScale, targetSize, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Para garantir que a escala final seja exatamente a escala desejada
        transitionCircle.localScale = targetSize;
    }

    //IEnumerator FadeCircle(Vector2 startSize, Vector2 finalSize)
    //{
    //    //transitionCircle.position = CirclePoint;
    //    transitionCircle.sizeDelta = startSize;
    //    while (true)
    //    {
    //        transitionCircle.sizeDelta = Vector2.Lerp(startSize, finalSize, transitionSpeed * Time.deltaTime);

    //        if(transitionCircle.sizeDelta == finalSize)
    //        {
    //            break;
    //        }

    //        yield return null;
    //    }
    //    transitionCircle.sizeDelta = finalSize;
    //}
}
