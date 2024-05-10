using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Scene Config")]
    public string sceneName = "Game";
    [SerializeField] public InputActionProperty startAction;
    [SerializeField] public InputActionProperty quitAction;
    public Button startButton;
    public Button quitButton;

    [Header("Audio")]
    public AudioClip[] AudioClip;
    public AudioSource audioSource;
    public float fadeInDuration = 2.0f;
    [Range(0, 1)] public float Volume = 1f;
    private int currentAudioIndex = 0;

    private void OnEnable()
    {
        startAction.action.Enable();
        startAction.action.performed += OnStartActionTriggered;

        quitAction.action.Enable();
        quitAction.action.performed += OnQuitActionTriggered;
    }

    private void OnDisable()
    {
        startAction.action.Disable();
        startAction.action.performed -= OnStartActionTriggered;

        quitAction.action.Disable();
        quitAction.action.performed -= OnQuitActionTriggered;
    }

    private void Start()
    {
        StartCoroutine(FadeInMusic());
        PlayNextAudioClip();
        startButton.onClick.AddListener(OnStartButton);
        quitButton.onClick.AddListener(OnQuitButton);
    }

    void Update()
    {
        jamLogo.transform.Rotate(0, logoRotationSpeed * Time.deltaTime, 0);

        if (audioSource != null && !audioSource.isPlaying)
        {
            PlayNextAudioClip();
        }
    }

    //Música
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

    private void PlayNextAudioClip()
    {
        if (AudioClip.Length > 0 && currentAudioIndex < AudioClip.Length)
        {
            audioSource.clip = AudioClip[currentAudioIndex];
            audioSource.Play();
            currentAudioIndex = (currentAudioIndex + 1) % AudioClip.Length;
        }
    }

    //StartQuitGame
    public void LoadScene()
    {
        Debug.Log("OnStartButton was called.");
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("OnQuitButton was called.");
        Application.Quit();
    }

    public void OnStartButton()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }

    public void OnQuitButton()
    {
        QuitGame();
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        StartCoroutine(FadeOutMusic());
        yield return new WaitForSeconds(fadeInDuration);
        LoadScene();
    }

    private void OnStartActionTriggered(InputAction.CallbackContext context)
    {
        OnStartButton();
    }

    private void OnQuitActionTriggered(InputAction.CallbackContext context)
    {
        OnQuitButton();
    }
}
