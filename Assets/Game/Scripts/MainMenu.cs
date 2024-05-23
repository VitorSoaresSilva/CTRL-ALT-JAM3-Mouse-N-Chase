using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;
//using UnityEditor.SearchService;
using UnityEngine.EventSystems;

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

    [SerializeField] private GameObject fastResponseBtn;
    [SerializeField] private GameObject pursuitBtn;
    [SerializeField] private GameObject rescueBtn;
    [SerializeField] private GameObject bossBtn;
    [SerializeField] private TextMeshProUGUI fastResponseQnt;
    [SerializeField] private TextMeshProUGUI rescueQnt;
    [SerializeField] private TextMeshProUGUI pursuitQnt;
    [SerializeField] private TextMeshProUGUI bossQnt;

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

            if (fastResponseBtn != null)
                fastResponseBtn.GetComponent<Button>().enabled = (CareerPoints.instance.FastResponseCompleted < 10);

            if (pursuitBtn != null)
                pursuitBtn.GetComponent<Button>().enabled = (CareerPoints.instance.PursuitCompleted < 10);

            if (rescueBtn != null)
                rescueBtn.GetComponent<Button>().enabled = (CareerPoints.instance.RescueCompleted < 10);

            if (bossBtn != null)
                bossBtn.GetComponent<Button>().enabled = (CareerPoints.instance.BossCompleted < 1);

            if (fastResponseQnt != null)
                fastResponseQnt.text = $"{CareerPoints.instance.FastResponseCompleted} / 10";

            if (pursuitQnt != null)
                pursuitQnt.text = $"{CareerPoints.instance.PursuitCompleted} / 10";

            if (rescueQnt != null)
                rescueQnt.text = $"{CareerPoints.instance.RescueCompleted} / 10";

            if (bossQnt != null)
                bossQnt.text = $"{CareerPoints.instance.BossCompleted} / 1";

            if(SceneControl.instance != null)
                SceneControl.instance.ToggleLoading(false);
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

    // Called by Start Game button
    public void ChangeScene(string scene)
    {
        StartCoroutine(FadeOutAndLoadScene(scene));
        FindObjectOfType<EventSystem>().enabled = false;
    }

    // Called by mission buttons
    public void ChangeToRandomScene(UIButton MissionBtn)
    {
        StartCoroutine(FadeAndLoadBiome(MissionBtn.missionType));
        FindObjectOfType<EventSystem>().enabled = false;
    }

    #region Private Methods
    private IEnumerator FadeOutAndLoadScene(string scene)
    {
        if (SceneControl.instance != null)
        {
            SceneControl.instance.ToggleLoading(true);
            StartCoroutine(FadeOutMusic());
            yield return new WaitForSeconds(fadeInDuration);
            SceneControl.instance.ChangeScene(scene);
        }
        yield return null;
        //SceneControl.instance.ToggleLoading(false);
    }

    private IEnumerator FadeAndLoadBiome(MissionType missionType)
    {
        if (SceneControl.instance != null)
        {
            SceneControl.instance.ToggleLoading(true);
            StartCoroutine(FadeOutMusic());
            yield return new WaitForSeconds(fadeInDuration);
            SceneControl.instance.LoadBiomeScene(missionType);

        }

        yield return null;
    }


    private IEnumerator FadeInMusic()
    {
        //float startTime = Time.time;

        //while (Time.time < startTime + fadeInDuration)
        //{
        //    float t = (Time.time - startTime) / fadeInDuration;
        //    audioSource.volume = t * Volume;

        //    yield return null;
        //}
        //audioSource.volume = Volume;

        float elapsedTime = 0;
        float startVolume = audioSource.volume;
        while (elapsedTime < fadeInDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 1, elapsedTime / fadeInDuration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }

    private IEnumerator FadeOutMusic()
    {
        //float startTime = Time.time;

        //while (Time.time < startTime + fadeInDuration)
        //{
        //    float t = (Time.time - startTime) / fadeInDuration;
        //    audioSource.volume = Volume - t * Volume;

        //    yield return null;
        //}
        //audioSource.volume = 0;

        float elapsedTime = 0;
        float startVolume = audioSource.volume;
        while (elapsedTime < fadeInDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeInDuration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }

    private void PlayNextAudioClip()
    {
        if (AudioClips.Length > 0)
        {
            int rndIndex = Random.Range(0, AudioClips.Length);
            if (rndIndex == currentAudioIndex)
            {
                rndIndex = (rndIndex + 1) % AudioClips.Length;
            }
            audioSource.clip = AudioClips[rndIndex];
            currentAudioIndex = rndIndex;
            audioSource.Play();
        }
    }
    #endregion

}