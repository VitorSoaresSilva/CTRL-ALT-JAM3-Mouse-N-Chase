using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//By FJB and Romeu
public class MainMenu : MonoBehaviour
{
    [Header("Jam Logo")]
    [SerializeField] private RectTransform jamLogo;
    [SerializeField] private float logoRotationSpeed = 50f;

    [Header("Game Scene Config")]
    public string sceneName = "Game";

    [Header("Components")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button quitButton;

    [Header("Audio")]
    public AudioClip[] AudioClip;
    public AudioSource audioSource;
    public float fadeInDuration = 2.0f;
    [Range(0, 1)] public float Volume = 1f;
    private int currentAudioIndex = 0;

    #region Unity Methods
    private void OnEnable()
    {
            
    }

    private void OnDisable()
    {

    }

    private void Start()
    {
        StartCoroutine(FadeInMusic());
        PlayNextAudioClip();
        startButton.Select();
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        jamLogo.transform.Rotate(0, logoRotationSpeed * Time.deltaTime, 0);

        if (audioSource != null && !audioSource.isPlaying)
        {
            PlayNextAudioClip();
        }
    }
    #endregion

    public void StartGame()
    {
        Debug.Log("OnStartButton was called.");
        StartCoroutine(FadeOutAndLoadScene());
    }

    public void QuitGame()
    {
        Debug.Log("OnQuitButton was called.");
        Application.Quit();
    }

    #region Private Methods
    private IEnumerator FadeOutAndLoadScene()
    {
        StartCoroutine(FadeOutMusic());
        yield return new WaitForSeconds(fadeInDuration);
        SceneManager.LoadScene(sceneName);
    }

    private void PlayNextAudioClip()
    {
        if (AudioClip.Length > 0 && currentAudioIndex < AudioClip.Length)
        {
            audioSource.clip = AudioClip[currentAudioIndex];
            audioSource.Play();
            currentAudioIndex = (currentAudioIndex + 1) % AudioClip.Length;
        }
    }
    private IEnumerator FadeInMusic()
    {
        float startTime = Time.time;

        while (Time.time < startTime + fadeInDuration)
        {
            float t = (Time.time - startTime) / fadeInDuration;
            audioSource.volume = t * Volume;

            yield return null;
        }
        audioSource.volume = Volume;
    }

    private IEnumerator FadeOutMusic()
    {
        float startTime = Time.time;

        while (Time.time < startTime + fadeInDuration)
        {
            float t = (Time.time - startTime) / fadeInDuration;
            audioSource.volume = Volume - t * Volume;

            yield return null;
        }
        audioSource.volume = 0;
    }
    #endregion

}
