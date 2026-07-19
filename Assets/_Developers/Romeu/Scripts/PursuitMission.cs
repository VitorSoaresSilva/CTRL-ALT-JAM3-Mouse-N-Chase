using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PathCreation;

public class PursuitMission : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public bool allowChaoticTraffic = true; // Flag para permitir trânsito caótico
    public EnemyCarFollowPath[] enemies;

    public EnemySpawner enemySpawner;

    List<EnemyCarFollowPath> enemyInstances = new();
    public int destroyedEnemies = 0;

    // Timer e missão
    [SerializeField] private float missionTimeLimit = 180f; // Tempo limite em segundos (3 minutos)
    private float missionStartTime = 0f;
    private float timeRemaining = 0f;
    private bool missionEnded = false;
    private const float TimeWarningThreshold = 30f;

    [Header("Situation VO")]
    [SerializeField] private AudioClip alertStopClip;
    private AudioSource audioSource;
    private bool alertStopPlayed;

    // UI References
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI enemiesStatusText; // Mostra: X presos / Y total
    [SerializeField] private GameObject[] missionHudVisuals;

    // Pursuit temporary powerups (boost + slow, no accumulate)
    [SerializeField] private GameObject pursuitSpeedPowerUpPrefab;
    [SerializeField] private GameObject pursuitSlowPowerUpPrefab;
    [SerializeField] private int minPowerUps = 2;
    [SerializeField] private int maxPowerUps = 3;
    [SerializeField] private float heightOffset = 1f;
    private readonly List<GameObject> spawnedSpeedPowerUps = new();
    private Transform powerUpHolder;
    private PathCreator subscribedPathCreator;

    // Proteção contra múltiplas chamadas de falha
    public bool IsMissionEnded => missionEnded;

    void OnEnable()
    {
        if (SceneControl.instance != null)
        {
            if (SceneControl.instance.currentMission != MissionType.Pursuit)
            {
                Destroy(this.gameObject);
            }
        }
    }

    void OnDisable()
    {
        UnsubscribePath();
        CleanupSpawnedPowerUps();
    }

    void Start()
    {
        enemyInstances.Clear();

        if (gameplayManager == null)
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }
        if(enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        // Randomizar tempo limite: 120-180 segundos (2-3 minutos)
        missionTimeLimit = Random.Range(120f, 180f);
        missionStartTime = Time.time;
        timeRemaining = missionTimeLimit;
        alertStopPlayed = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Debug.Log($"PursuitMission started - Time limit: {missionTimeLimit:F1}s");

        enemyInstances.AddRange(enemySpawner.SpawnEnemies(enemies));

        foreach (EnemyCarFollowPath enemy in enemyInstances)
        {
            int hits = 0;
            enemy.damage.onDamage += () =>
            {
                hits++;
                Debug.Log($"[Pursuit] Hit no {enemy.name}: {hits}/5");
            };

            enemy.damage.onDie = () =>
            {
                if (missionEnded)
                    return;

                destroyedEnemies++;
                enemy.gameObject.SetActive(false);
                UpdateEnemiesStatusDisplay();

                Debug.Log($"Enemy captured! {destroyedEnemies}/{enemyInstances.Count}");

                if (destroyedEnemies >= enemyInstances.Count)
                {
                    missionEnded = true;
                    OnMissionEnd();
                    gameplayManager.EndGameplay(true);
                }
            };
        }

        UpdateEnemiesStatusDisplay();
        ActivateMissionHuds(true);

        StartCoroutine(EnsurePursuitSpeedPowerUpSpawning());
    }

    void Update()
    {
        if (missionEnded)
            return;

        timeRemaining = missionTimeLimit - (Time.time - missionStartTime);
        UpdateTimerDisplay();
        TryPlayTimeWarning();

        if (timeRemaining <= 0)
        {
            missionEnded = true;
            Debug.Log("Mission failed - Time limit exceeded!");
            FailMission();
            return;
        }
    }

    private void TryPlayTimeWarning()
    {
        if (alertStopPlayed || timeRemaining > TimeWarningThreshold)
            return;

        alertStopPlayed = true;
        if (audioSource != null && alertStopClip != null)
            audioSource.PlayOneShot(alertStopClip);
    }

    private IEnumerator EnsurePursuitSpeedPowerUpSpawning()
    {
        if (pursuitSpeedPowerUpPrefab == null && pursuitSlowPowerUpPrefab == null)
        {
            Debug.LogWarning("[Pursuit] Nenhum prefab de powerup configurado!");
            yield break;
        }

        yield return new WaitUntil(() =>
            PathGenerator.instance != null
            && PathGenerator.instance.pathCreatorInstance != null
            && PathGenerator.instance.pathCreatorInstance.path != null
            && PathGenerator.instance.pathCreatorInstance.path.length > 10f);

        if (missionEnded)
            yield break;

        // Spawner global: mantém Vida/Shield, remove Speed acumulativo
        ConfigureGlobalSpawnerForPursuit();

        // Holder como o da Vida (MultipleObjectSpawner)
        if (powerUpHolder == null)
        {
            var holderGo = new GameObject("PursuitPowerUpHolder");
            powerUpHolder = holderGo.transform;
        }

        PathCreator pathCreator = PathGenerator.instance.pathCreatorInstance;
        SubscribePath(pathCreator);
        SpawnPowerUpsAlongPath();
    }

    private void ConfigureGlobalSpawnerForPursuit()
    {
        MultipleObjectSpawner globalSpawner = FindObjectOfType<MultipleObjectSpawner>();
        if (globalSpawner == null) return;

        // Remove só SpeedPowerUp acumulativo; mantém Vida/Shield no path
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

    private void SubscribePath(PathCreator pathCreator)
    {
        if (subscribedPathCreator == pathCreator) return;

        UnsubscribePath();
        subscribedPathCreator = pathCreator;
        if (subscribedPathCreator != null)
            subscribedPathCreator.pathUpdated += OnPathUpdated;
    }

    private void UnsubscribePath()
    {
        if (subscribedPathCreator != null)
        {
            subscribedPathCreator.pathUpdated -= OnPathUpdated;
            subscribedPathCreator = null;
        }
    }

    private void OnPathUpdated()
    {
        if (missionEnded) return;
        // Quando o path regenera (novo ponto/lap), respawna no path novo — como a Vida
        SpawnPowerUpsAlongPath();
    }

    private void SpawnPowerUpsAlongPath()
    {
        CleanupSpawnedPowerUps();

        PathGenerator pathGenerator = PathGenerator.instance;
        if (pathGenerator == null || pathGenerator.pathCreatorInstance == null) return;

        VertexPath path = pathGenerator.pathCreatorInstance.path;
        if (path == null || path.length <= 10f) return;

        int count = Random.Range(minPowerUps, maxPowerUps + 1);
        float minDist = path.length * 0.2f;
        float maxDist = path.length * 0.85f;
        float minGap = Mathf.Max(path.length * 0.2f, 120f); // bem espaçados

        var usedDistances = new List<float>();

        // Garante pelo menos 1 slow (retardo) para dificultar — o resto aleatório
        var prefabOrder = BuildPrefabSpawnList(count);

        for (int i = 0; i < prefabOrder.Count; i++)
        {
            float dst = 0f;
            bool ok = false;
            for (int attempt = 0; attempt < 12; attempt++)
            {
                dst = Random.Range(minDist, maxDist);
                ok = true;
                foreach (float used in usedDistances)
                {
                    if (Mathf.Abs(dst - used) < minGap)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok) break;
            }
            if (!ok) continue;

            usedDistances.Add(dst);

            GameObject prefab = prefabOrder[i];
            if (prefab == null) continue;

            // Mesmo estilo da Vida / MultipleObjectSpawner
            Vector3 point = path.GetPointAtDistance(dst, EndOfPathInstruction.Loop);
            Quaternion rot = path.GetRotationAtDistance(dst, EndOfPathInstruction.Loop);
            Vector3 normal = path.GetNormalAtDistance(dst, EndOfPathInstruction.Loop);
            point += normal * Random.Range(-1.5f, 1.5f);
            point.y += heightOffset;

            Quaternion rotation = rot * Quaternion.Euler(0f, 0f, 90f);
            Transform parent = powerUpHolder != null ? powerUpHolder : null;
            GameObject powerUp = Instantiate(prefab, point, rotation, parent);
            spawnedSpeedPowerUps.Add(powerUp);
        }

        Debug.Log($"[Pursuit] PowerUps no path: {spawnedSpeedPowerUps.Count} (inclui retardo garantido)");
    }

    /// <summary>
    /// Monta a lista de prefabs: 1 slow garantido + resto com chance maior de slow (~60%).
    /// Embaralha para o retardo não ficar sempre no mesmo “slot”.
    /// </summary>
    private List<GameObject> BuildPrefabSpawnList(int count)
    {
        var list = new List<GameObject>();
        bool hasSpeed = pursuitSpeedPowerUpPrefab != null;
        bool hasSlow = pursuitSlowPowerUpPrefab != null;

        if (!hasSpeed && !hasSlow) return list;

        if (hasSlow)
            list.Add(pursuitSlowPowerUpPrefab); // pelo menos 1 retardo

        while (list.Count < count)
        {
            if (hasSpeed && hasSlow)
            {
                // 60% slow / 40% boost — dificulta mais
                list.Add(Random.value < 0.6f ? pursuitSlowPowerUpPrefab : pursuitSpeedPowerUpPrefab);
            }
            else if (hasSlow)
                list.Add(pursuitSlowPowerUpPrefab);
            else
                list.Add(pursuitSpeedPowerUpPrefab);
        }

        // Embaralha ordem
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    private void CleanupSpawnedPowerUps()
    {
        foreach (GameObject powerUp in spawnedSpeedPowerUps)
        {
            if (powerUp != null)
                Destroy(powerUp);
        }
        spawnedSpeedPowerUps.Clear();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = (int)(timeRemaining / 60f);
            int seconds = (int)(timeRemaining % 60f);
            timerText.text = $"{minutes:D2}:{seconds:D2}";

            if (timeRemaining <= TimeWarningThreshold)
                timerText.color = Color.red;
            else
                timerText.color = Color.white;
        }
    }

    private void UpdateEnemiesStatusDisplay()
    {
        if (enemiesStatusText != null)
        {
            // Garante label correta (HUD compartilhado com BossMission)
            var label = enemiesStatusText.transform.parent != null
                ? enemiesStatusText.transform.parent.GetComponent<TextMeshProUGUI>()
                : null;
            if (label != null)
                label.text = "Total Enemy:";

            enemiesStatusText.text = $"{destroyedEnemies}/{enemyInstances.Count}";
        }
    }

    private void FailMission()
    {
        StopAllCoroutines();
        OnMissionEnd();

        if (gameplayManager != null)
        {
            gameplayManager.EndGameplay(false);
        }
    }

    private void ActivateMissionHuds(bool activate)
    {
        if (missionHudVisuals == null || missionHudVisuals.Length == 0) return;

        foreach (GameObject hudVisual in missionHudVisuals)
        {
            if (hudVisual != null)
            {
                hudVisual.SetActive(activate);
            }
        }
    }

    private void OnMissionEnd()
    {
        UnsubscribePath();
        ActivateMissionHuds(false);
        CleanupSpawnedPowerUps();
        if (powerUpHolder != null)
            Destroy(powerUpHolder.gameObject);
        powerUpHolder = null;
    }
}
