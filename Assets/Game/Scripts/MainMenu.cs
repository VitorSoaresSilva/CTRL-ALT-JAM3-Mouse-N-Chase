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
    [SerializeField] private Button secretCarSlot;

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

        RefreshHubUI();
    }

    public void RefreshHubUI()
    {
        if (CareerPoints.instance == null)
            return;

        if (pointsText != null)
            pointsText.text = $"{CareerPoints.instance.Points}";

        if (completedMissionsText != null)
            completedMissionsText.text = $"{CareerPoints.instance.MissionsCompleted}";

        if (lostPointsText != null)
            lostPointsText.text = $"{CareerPoints.instance.LostPoints}";

        if (shieldSlot.LockedIcon != null && shieldSlot.UnlockedIcon != null)
        {
            shieldSlot.LockedIcon.gameObject.SetActive(!CareerPoints.instance.ShieldUnlocked);
            shieldSlot.UnlockedIcon.gameObject.SetActive(CareerPoints.instance.ShieldUnlocked);
        }

        if (bumperSlot.LockedIcon != null && bumperSlot.UnlockedIcon != null)
        {
            bumperSlot.LockedIcon.gameObject.SetActive(!CareerPoints.instance.BumperUnlocked);
            bumperSlot.UnlockedIcon.gameObject.SetActive(CareerPoints.instance.BumperUnlocked);
        }

        // slotSlot icons live under SecretCar in the Police Station scene —
        // drive them by SecretCarUnlocked, not SlotUnlocked (25k points).
        if (secretCarSlot != null)
        {
            bool unlocked = CareerPoints.instance.SecretCarUnlocked;
            secretCarSlot.gameObject.SetActive(unlocked);
            if (slotSlot.LockedIcon != null)
                slotSlot.LockedIcon.gameObject.SetActive(false);
            if (slotSlot.UnlockedIcon != null)
                slotSlot.UnlockedIcon.gameObject.SetActive(unlocked);

            UpdateSecretCarVisual();
        }

        // Upgrade tip column: Left/Right only (this game uses 2 keys)
        WireUpgradeTipNavigation();
        StartCoroutine(WireUpgradeTipNavigationEndOfFrame());

        if (fastResponseBtn != null)
        {
            Button fastResponseButton = fastResponseBtn.GetComponent<Button>();
            SetBtnLocked(fastResponseButton, !CareerPoints.instance.IsMissionUnlocked(MissionType.FastResponse));
        }
        if (pursuitBtn != null)
        {
            Button pursuitButton = pursuitBtn.GetComponent<Button>();
            SetBtnLocked(pursuitButton, !CareerPoints.instance.IsMissionUnlocked(MissionType.Pursuit));
        }
        if (rescueBtn != null)
        {
            Button rescueButton = rescueBtn.GetComponent<Button>();
            SetBtnLocked(rescueButton, !CareerPoints.instance.IsMissionUnlocked(MissionType.Rescue));
        }
        if (bossBtn != null)
        {
            Button bossButton = bossBtn.GetComponent<Button>();
            SetBtnLocked(bossButton, !CareerPoints.instance.IsMissionUnlocked(MissionType.Boss));
        }

        SetMissionProgressText(fastResponseQnt, CareerPoints.instance.FastResponseCompleted, CareerPoints.MissionQuota);
        SetMissionProgressText(pursuitQnt, CareerPoints.instance.PursuitCompleted, CareerPoints.MissionQuota);
        SetMissionProgressText(rescueQnt, CareerPoints.instance.RescueCompleted, CareerPoints.MissionQuota);
        SetMissionProgressText(bossQnt, CareerPoints.instance.BossCompleted, CareerPoints.BossQuota);

        if (SceneControl.instance != null)
            SceneControl.instance.ToggleLoading(false);
    }

    static void SetBtnLocked(Button btn, bool locked)
    {
        if (btn == null) return;
        btn.interactable = !locked;
    }

    IEnumerator WireUpgradeTipNavigationEndOfFrame()
    {
        yield return null;
        WireUpgradeTipNavigation();
        EnsureHubSelection();
    }

    void EnsureHubSelection()
    {
        var es = EventSystem.current;
        if (es == null)
            return;

        if (es.currentSelectedGameObject != null)
            return;

        Button first = null;
        if (fastResponseBtn != null)
            first = fastResponseBtn.GetComponent<Button>();
        if (first != null && first.interactable)
            first.Select();
    }

    /// <summary>
    /// Same pattern as Secret Car: real Unity Buttons + Explicit navigation.
    /// IMPORTANT: this project's UI Move action is horizontal-only (Left/Right),
    /// so the tip column is chained with selectOnRight / selectOnLeft — not Down.
    /// </summary>
    void WireUpgradeTipNavigation()
    {
        Transform shieldRoot = GetSlotRoot(shieldSlot) ?? FindNamed("ShieldUpgrade");
        Transform bumperRoot = GetSlotRoot(bumperSlot) ?? FindNamed("BumperUpgrade");

        Button shield = EnsurePlainUpgradeButton(shieldRoot);
        Button bumper = EnsurePlainUpgradeButton(bumperRoot);
        Button secret = secretCarSlot;
        Button boss = bossBtn != null ? bossBtn.GetComponent<Button>() : null;

        Button duty = null;
        var dutyGo = GameObject.Find("DutyButton");
        if (dutyGo != null)
            duty = dutyGo.GetComponent<Button>();

        CleanupCustomUpgradeNav(shieldRoot);
        CleanupCustomUpgradeNav(bumperRoot);
        if (secret != null)
            CleanupCustomUpgradeNav(secret.transform);

        bool secretActive = secret != null && secret.gameObject.activeInHierarchy;

        if (shield != null) shield.interactable = true;
        if (bumper != null) bumper.interactable = true;
        if (secret != null) secret.interactable = true;

        Selectable afterBumper = secretActive ? (Selectable)secret : duty;
        SetExplicitNav(shield, left: boss, right: bumper, up: null, down: null);
        SetExplicitNav(bumper, left: shield, right: afterBumper, up: null, down: null);

        if (secret != null)
        {
            SetExplicitNav(secret,
                left: bumper,
                right: duty,
                up: null,
                down: null);
        }

        if (boss != null && shield != null)
        {
            Navigation nav = boss.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnRight = shield;
            boss.navigation = nav;
        }

        if (duty != null)
        {
            Navigation nav = duty.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnLeft = secretActive ? secret : (bumper != null ? (Selectable)bumper : shield);
            duty.navigation = nav;
        }

        EnsureUpgradeUIButton(shieldRoot);
        EnsureUpgradeUIButton(bumperRoot);
    }

    static void CleanupCustomUpgradeNav(Transform root)
    {
        if (root == null)
            return;
        foreach (var nav in root.GetComponents<HubUpgradeNav>())
            Destroy(nav);
        foreach (var hub in root.GetComponents<HubUpgradeButton>())
            Destroy(hub);
    }

    static void SetExplicitNav(Button button, Selectable left, Selectable right, Selectable up, Selectable down)
    {
        if (button == null)
            return;

        button.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnLeft = left,
            selectOnRight = right,
            selectOnUp = up,
            selectOnDown = down
        };
        button.interactable = true;
    }

    static Transform GetSlotRoot(powerupSlot slot)
    {
        if (slot.LockedIcon != null)
            return slot.LockedIcon.transform.parent;
        if (slot.UnlockedIcon != null)
            return slot.UnlockedIcon.transform.parent;
        return null;
    }

    static Transform FindNamed(string objectName)
    {
        var go = GameObject.Find(objectName);
        return go != null ? go.transform : null;
    }

    static Button EnsurePlainUpgradeButton(Transform root)
    {
        if (root == null)
            return null;

        CleanupCustomUpgradeNav(root);

        var button = root.GetComponent<Button>();
        if (button == null)
            button = root.gameObject.AddComponent<Button>();

        var image = root.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
            button.targetGraphic = image;
        }

        button.transition = Selectable.Transition.ColorTint;
        button.interactable = true;
        return button;
    }

    static void EnsureUpgradeUIButton(Transform root)
    {
        if (root == null)
            return;

        var uiBtn = root.GetComponent<UIButton>();
        if (uiBtn == null)
            uiBtn = root.gameObject.AddComponent<UIButton>();

        var pointer = FindObjectOfType<UIPointer>();
        var selfRt = root as RectTransform;
        var pointerRt = pointer != null ? pointer.GetComponent<RectTransform>() : null;
        var pointerParent = pointerRt != null ? pointerRt.parent as RectTransform : null;
        if (pointer == null || selfRt == null || pointerParent == null)
            return;

        Vector3[] corners = new Vector3[4];
        selfRt.GetWorldCorners(corners);
        Vector3 leftCenter = (corners[0] + corners[1]) * 0.5f;

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, leftCenter);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(pointerParent, screen, null, out Vector2 local))
            return;

        local.x -= 36f;
        uiBtn.SetPointerAnchor(pointer, local, new Vector3(0f, 0f, -90f));
    }

    static void SetMissionProgressText(TextMeshProUGUI label, int completed, int max)
    {
        if (label == null) return;

        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Overflow;
        label.text = $"{Mathf.Min(completed, max)}/{max}";
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

    public void ToggleSecretCar()
    {
        if (CareerPoints.instance == null)
            return;

        CareerPoints.instance.usingSecretCar = !CareerPoints.instance.usingSecretCar;
        UpdateSecretCarVisual();
    }

    void UpdateSecretCarVisual()
    {
        if (secretCarSlot == null || CareerPoints.instance == null)
            return;

        bool on = CareerPoints.instance.usingSecretCar;
        Color accent = on ? new Color(0.35f, 0.6f, 1f, 1f) : new Color(1f, 0.35f, 0.35f, 1f);
        Color buttonTint = on ? new Color(0.65f, 0.78f, 1f, 1f) : new Color(1f, 0.72f, 0.72f, 1f);

        var secretLabel = secretCarSlot.GetComponentInChildren<TextMeshProUGUI>(true);
        if (secretLabel != null)
            secretLabel.color = accent;

        if (slotSlot.UnlockedIcon != null)
            slotSlot.UnlockedIcon.color = accent;

        if (secretCarSlot.targetGraphic != null)
            secretCarSlot.targetGraphic.color = buttonTint;
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
    }

    // Called by mission buttons
    public void ChangeToRandomScene(UIButton MissionBtn)
    {
        StartCoroutine(FadeAndLoadBiome(MissionBtn.missionType));
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
        if (audioSource == null)
            yield break;

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
        if (AudioClips == null || AudioClips.Length == 0 || audioSource == null)
            return;

        int rndIndex = Random.Range(0, AudioClips.Length);
        if (rndIndex == currentAudioIndex)
        {
            rndIndex = (rndIndex + 1) % AudioClips.Length;
        }
        audioSource.clip = AudioClips[rndIndex];
        currentAudioIndex = rndIndex;
        audioSource.Play();
    }
    #endregion

}