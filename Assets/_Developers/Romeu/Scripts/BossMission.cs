using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BossMission : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public bool allowChaoticTraffic = true;
    public EnemyCarFollowPath[] enemies;
    public EnemySpawner enemySpawner;

    [Header("Boss combat")]
    [SerializeField] private int hitsToWin = 10;
    [SerializeField] private GameObject barrelThrowablePrefab;
    [SerializeField] private GameObject pianoHazardPrefab;
    [SerializeField] private GameObject dropEffectPrefab;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI hitsStatusText;
    [SerializeField] private TextMeshProUGUI hitsLabelText;
    [SerializeField] private GameObject[] missionHudVisuals;
    [SerializeField] private string hitsLabel = "Hits in Boss:";
    [SerializeField] private string defaultEnemyLabel = "Total Enemy:";

    List<EnemyCarFollowPath> enemyInstances = new();
    public int destroyedEnemies = 0;
    public int hitsLanded = 0;

    private bool missionEnded;
    private Transform dropHolder;
    private BossDropController dropController;

    public bool IsMissionEnded => missionEnded;

    void OnEnable()
    {
        if (SceneControl.instance != null)
        {
            if (SceneControl.instance.currentMission != MissionType.Boss)
            {
                Destroy(this.gameObject);
            }
        }
    }

    void OnDisable()
    {
        if (dropController != null)
            dropController.StopDrops();
        RestoreHudLabel();
    }

    void Start()
    {
        enemyInstances.Clear();
        hitsLanded = 0;
        destroyedEnemies = 0;
        missionEnded = false;

        if (gameplayManager == null)
            gameplayManager = FindObjectOfType<GameplayManager>();
        if (enemySpawner == null)
            enemySpawner = FindObjectOfType<EnemySpawner>();

        enemyInstances.AddRange(enemySpawner.SpawnEnemies(enemies));

        foreach (EnemyCarFollowPath enemy in enemyInstances)
        {
            enemy.ConfigureBossPacing();

            if (enemy.damage != null)
            {
                enemy.damage.ignorePlayerRamming = true;
                enemy.damage.maxHits = hitsToWin;
                enemy.damage.onDamage += OnBossHit;
                enemy.damage.onDie = OnBossDefeated;
            }

            dropController = enemy.GetComponent<BossDropController>();
            if (dropController == null)
                dropController = enemy.gameObject.AddComponent<BossDropController>();
        }

        UpdateHitsDisplay();
        ApplyBossHudLabel();
        ActivateMissionHuds(true);
        StartCoroutine(SetupBossCombat());
    }

    private IEnumerator SetupBossCombat()
    {
        yield return new WaitUntil(() =>
            PathGenerator.instance != null
            && PathGenerator.instance.pathCreatorInstance != null
            && PathGenerator.instance.pathCreatorInstance.path != null
            && PathGenerator.instance.pathCreatorInstance.path.length > 10f);

        if (missionEnded)
            yield break;

        ConfigureGlobalSpawnerForBoss();

        if (dropHolder == null)
        {
            var holderGo = new GameObject("BossDropHolder");
            dropHolder = holderGo.transform;
        }

        Transform playerTransform = gameplayManager != null && gameplayManager.playerCar != null
            ? gameplayManager.playerCar.transform
            : null;

        EnemyDamage bossDamage = enemyInstances.Count > 0 ? enemyInstances[0].damage : null;

        if (dropController != null)
        {
            dropController.Configure(
                barrelThrowablePrefab,
                pianoHazardPrefab,
                dropEffectPrefab,
                playerTransform,
                bossDamage,
                dropHolder);
            dropController.BeginDrops();
        }
    }

    private void OnBossHit()
    {
        if (missionEnded)
            return;

        hitsLanded++;
        UpdateHitsDisplay();
    }

    private void OnBossDefeated()
    {
        if (missionEnded)
            return;

        missionEnded = true;
        destroyedEnemies++;

        foreach (EnemyCarFollowPath enemy in enemyInstances)
        {
            if (enemy != null)
                enemy.gameObject.SetActive(false);
        }

        OnMissionEnd();
        if (gameplayManager != null)
            gameplayManager.EndGameplay(true);
    }

    private void ConfigureGlobalSpawnerForBoss()
    {
        MultipleObjectSpawner globalSpawner = FindObjectOfType<MultipleObjectSpawner>();
        if (globalSpawner == null)
            return;

        // Mantem Vida/Shield; remove SpeedPowerUp acumulativo
        if (globalSpawner.prefabs != null && globalSpawner.prefabs.Length > 0)
        {
            var filtered = new List<GameObject>();
            foreach (GameObject prefab in globalSpawner.prefabs)
            {
                if (prefab == null) continue;
                if (prefab.GetComponent<SpeedPowerUp>() != null) continue;
                filtered.Add(prefab);
            }
            if (filtered.Count > 0)
                globalSpawner.prefabs = filtered.ToArray();
        }

        globalSpawner.gameObject.SetActive(true);
    }

    private void UpdateHitsDisplay()
    {
        if (hitsStatusText != null)
            hitsStatusText.text = $"{hitsLanded}/{hitsToWin}";
    }

    private void ApplyBossHudLabel()
    {
        ResolveHitsLabel();
        if (hitsLabelText != null)
            hitsLabelText.text = hitsLabel;
    }

    private void RestoreHudLabel()
    {
        ResolveHitsLabel();
        if (hitsLabelText != null)
            hitsLabelText.text = defaultEnemyLabel;
    }

    private void ResolveHitsLabel()
    {
        if (hitsLabelText != null || hitsStatusText == null)
            return;

        // TotalEnemyText é filho do TotalEnemyLabel no canvas compartilhado
        hitsLabelText = hitsStatusText.transform.parent != null
            ? hitsStatusText.transform.parent.GetComponent<TextMeshProUGUI>()
            : null;
    }

    private void ActivateMissionHuds(bool activate)
    {
        if (missionHudVisuals == null || missionHudVisuals.Length == 0)
            return;

        foreach (GameObject hudVisual in missionHudVisuals)
        {
            if (hudVisual != null)
                hudVisual.SetActive(activate);
        }
    }

    private void OnMissionEnd()
    {
        if (dropController != null)
            dropController.StopDrops();

        RestoreHudLabel();
        ActivateMissionHuds(false);

        if (dropHolder != null)
        {
            Destroy(dropHolder.gameObject);
            dropHolder = null;
        }
    }
}
