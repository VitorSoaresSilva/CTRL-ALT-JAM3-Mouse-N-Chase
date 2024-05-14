using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

//By FJB and Romeu
public class MainMenu : MonoBehaviour
{
    [Header("Jam Logo")]
    [SerializeField] private RectTransform jamLogo;
    [SerializeField] private float logoRotationSpeed = 50f;

    [Header("Game Scene Config")]
    public string sceneName = "Game";

    [Header("Audio"), FormerlySerializedAs("AudioClip")]
    public AudioClip[] AudioClips;
    public AudioSource audioSource;
    public float fadeInDuration = 2.0f;
    [Range(0, 1)] public float Volume = 1f;
    private int currentAudioIndex = 0;

    [SerializeField, Header("Controls Dialog")]
    private RectTransform controlsDialog;
    [SerializeField] private Button openDialogBtn;
    [SerializeField] private Button closeDialogBtn;

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
    }

    void Update()
    {
        if(jamLogo != null)
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
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    public void ChangeScene(string scene)
    {
        StartCoroutine(FadeOutAndLoadScene(scene));
    }

    public void QuitGame()
    {
        Debug.Log("OnQuitButton was called.");
        Application.Quit();
    }

    public void ShowControls()
    {
        if(controlsDialog != null)
            controlsDialog.gameObject.SetActive(true);
        if(closeDialogBtn != null)
            closeDialogBtn.Select();
    }

    public void HideControls()
    {
        if (controlsDialog != null)
            controlsDialog.gameObject.SetActive(false);
        if(openDialogBtn != null)
            openDialogBtn.Select();
    }

    #region Private Methods
    private IEnumerator FadeOutAndLoadScene(string scene)
    {
        StartCoroutine(FadeOutMusic());
        yield return new WaitForSeconds(fadeInDuration);
        SceneManager.LoadScene(scene);
    }

    private void PlayNextAudioClip()
    {
        if (AudioClips.Length > 0)
        {
            int rndIndex = Random.Range(0, AudioClips.Length);
            if(rndIndex == currentAudioIndex)
            {
                rndIndex = (rndIndex + 1) % AudioClips.Length;
            }
            audioSource.clip = AudioClips[rndIndex];
            currentAudioIndex = rndIndex;
            audioSource.Play();
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
