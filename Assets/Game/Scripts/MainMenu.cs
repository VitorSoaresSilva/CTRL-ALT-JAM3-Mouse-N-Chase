using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SearchService;

//By FJB and Romeu
public class MainMenu : MonoBehaviour
{
    [Header("Jam Logo")]
    [SerializeField] private RectTransform jamLogo;
    [SerializeField] private float logoRotationSpeed = 50f;

    [Header("Game Scene Config")]
    public string sceneName = "Game";
    public string[] gameScenes = new string[] { "BiomeCorrupted", "BiomeDesert", "BiomeFlorest", "BiomeMix" };

    [Header("Audio"), FormerlySerializedAs("AudioClip")]
    public AudioClip[] AudioClips;
    public AudioSource audioSource;
    public float fadeInDuration = 2.0f;
    [Range(0, 1)] public float Volume = 1f;
    private int currentAudioIndex = 0;

    [Header("Background Image")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgroundImages;

    [SerializeField, Header("Credits")]
    private ScrollRect creditsScrollRect;
    [SerializeField] private float creditsScrollSpeed = 0.035f;
    private bool creditsScrollDirection = false;

    [SerializeField, Header("Points")]
    private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI completedMissionsText;
    [SerializeField] private TextMeshProUGUI lostPointsText;

    [System.Serializable]
    public struct powerupSlot { public Image LockedIcon; public Image UnlockedIcon; }
    [SerializeField, Header("Upgrades")] private powerupSlot shieldSlot;
    [SerializeField] private powerupSlot slotSlot;
    [SerializeField] private powerupSlot bumperSlot;


    #region Unity Methods
    private void OnEnable()
    {
            
    }

    private void OnDisable()
    {

    }

    private void Start()
    {
        SetRandomBackgroundImage();
        StartCoroutine(FadeInMusic());
        PlayNextAudioClip();

        if(creditsScrollRect)
            creditsScrollRect.verticalNormalizedPosition = (creditsScrollDirection) ? 0 : 1;

        if(CareerPoints.instance != null)
        {
            Debug.Log("Showing Career Points");
            if(pointsText != null)
                pointsText.text = $"{CareerPoints.instance.Points}";

            if (completedMissionsText != null)
                completedMissionsText.text = $"{CareerPoints.instance.MissionsCompleted}";

            if (lostPointsText != null)
                lostPointsText.text = $"{CareerPoints.instance.LostPoints}";

            if(shieldSlot.LockedIcon != null && shieldSlot.UnlockedIcon != null)
            {
                shieldSlot.LockedIcon.gameObject.SetActive(!CareerPoints.instance.ShieldUnlocked);
                shieldSlot.UnlockedIcon.gameObject.SetActive(CareerPoints.instance.ShieldUnlocked);
            }

            if (slotSlot.LockedIcon != null && slotSlot.UnlockedIcon != null)
            {
                slotSlot.LockedIcon.gameObject.SetActive(!CareerPoints.instance.SlotUnlocked);
                slotSlot.UnlockedIcon.gameObject.SetActive(CareerPoints.instance.SlotUnlocked);
            }

            if (bumperSlot.LockedIcon != null && bumperSlot.UnlockedIcon != null)
            {
                bumperSlot.LockedIcon.gameObject.SetActive(!CareerPoints.instance.BumperUnlocked);
                bumperSlot.UnlockedIcon.gameObject.SetActive(CareerPoints.instance.BumperUnlocked);
            }
        }

    }

    void Update()
    {
        if(jamLogo != null)
            jamLogo.transform.Rotate(0, logoRotationSpeed * Time.deltaTime, 0);

        if (audioSource != null && !audioSource.isPlaying)
        {
            PlayNextAudioClip();
        }

        // credits auto scroll
        if(creditsScrollRect != null && creditsScrollRect.gameObject.activeSelf)
        {
            creditsScrollRect.verticalNormalizedPosition = Mathf.MoveTowards(
                creditsScrollRect.verticalNormalizedPosition, 
                (creditsScrollDirection) ? 0 : 1,
                Time.deltaTime * creditsScrollSpeed
            );

            if (creditsScrollRect.verticalNormalizedPosition <= 0.01f)
                creditsScrollDirection = false;
            else if(creditsScrollRect.verticalNormalizedPosition >= 0.99f)
                creditsScrollDirection = true;
        }        
    }
    #endregion

    private void SetRandomBackgroundImage()
    {
        if (backgroundImages.Length > 0 && backgroundImage != null)
        {
            int index = Random.Range(0, backgroundImages.Length);
            backgroundImage.sprite = backgroundImages[index];
        }
    }

    public void StartGame()
    {
        Debug.Log("OnStartButton was called.");
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }
    public void QuitGame()
    {
        Debug.Log("OnQuitButton was called.");
        Application.Quit();
    }

    public void ChangeScene(string scene)
    {
        StartCoroutine(FadeOutAndLoadScene(scene));
    }

    public void ChangeToRandomScene()
    {
        string chosenScene = gameScenes[Random.Range(0, gameScenes.Length)];
        StartCoroutine(FadeAndLoadBiome());
        // inserir alguma lógica relativa a missão
    }

    #region Private Methods
    private IEnumerator FadeAndLoadBiome()
    {
        StartCoroutine(FadeOutMusic());

        if (Fade.instance != null)
            Fade.instance.FadeIn();

        yield return new WaitForSeconds(fadeInDuration);

        if (SceneControl.instance != null)
            SceneControl.instance.LoadBiomeScene();
        else
        {   // fallback
            SceneManager.LoadSceneAsync(gameScenes[Random.Range(0, gameScenes.Length)]);
            SceneManager.LoadSceneAsync(sceneName);
        }
    }

    private IEnumerator FadeOutAndLoadScene(string scene)
    {
        StartCoroutine(FadeOutMusic());

        if(Fade.instance != null)
            Fade.instance.FadeIn();
            
        yield return new WaitForSeconds(fadeInDuration);

        if (SceneControl.instance != null)
            SceneControl.instance.LoadBiomeScene(); 
        else SceneManager.LoadSceneAsync(scene);
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