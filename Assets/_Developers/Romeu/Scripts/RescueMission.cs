using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PathCreation;

public class RescueMission : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public bool allowChaoticTraffic = false;
    public EnemyCarFollowPath[] enemies;
    public EnemySpawner enemySpawner;

    [Header("Hostage / follow")]
    [SerializeField] private float maxFollowDistance = 70f;
    [SerializeField] private float riskFollowDistance = 40f;
    [SerializeField] private float closeFollowDistance = 22f;
    [SerializeField] private float closeWorldDistance = 18f;
    [SerializeField] private float distanceGraceSeconds = 5f;
    [SerializeField] private float farFailDelay = 1.25f;
    [SerializeField] private float tunnelWinRadius = 40f;
    [SerializeField] private float tunnelWinPathDistance = 45f;

    [Header("Roadblock")]
    [SerializeField] private int lapsToRoadblock = 2;

    [Header("Timer")]
    [SerializeField] private float missionTimeLimit = 120f;
    [SerializeField] private float trafficHitTimePenalty = 5f;
    private float missionStartTime;
    private float timePenaltySeconds;
    private float timeRemaining;
    private bool missionEnded;
    private const float TimeWarningThreshold = 30f;

    [Header("Situation VO")]
    [SerializeField] private AudioClip alertStopClip;
    [SerializeField] private AudioClip timeOverClip;
    private AudioSource audioSource;
    private bool alertStopPlayed;
    private bool timeOverPlayed;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI statusLabelText;
    [SerializeField] private TextMeshProUGUI progressPercentageText;
    [SerializeField] private GameObject[] missionHudVisuals;

    [Header("Rescue NPC powerups")]
    [SerializeField] private GameObject rescueNpcBoostPrefab;
    [SerializeField] private GameObject rescueNpcSlowPrefab;
    [SerializeField] private int minPowerUpPairs = 3;
    [SerializeField] private int maxPowerUpPairs = 5;
    [SerializeField] private float sideOffset = 3.5f;
    [SerializeField] private float heightOffset = 1f;

    private readonly List<GameObject> spawnedPowerUps = new();
    private Transform powerUpHolder;
    private PathCreator subscribedPathCreator;

    private readonly List<EnemyCarFollowPath> enemyInstances = new();
    private EnemyCarFollowPath hostageCarrier;
    private readonly List<EnemyCarFollowPath> escorts = new();
    private int capturedEscorts;
    private bool roadblockActive;
    private bool tunnelSpawned;
    private float pathRegenGraceUntil;
    private float currentPathGap;
    private PlayerCar playerCar;

    private enum FollowZone { Safe, Close, Warning, Lost }
    private FollowZone followZone = FollowZone.Safe;
    private float farZoneEnterTime = -1f;

    public bool IsMissionEnded => missionEnded;
    public static EnemyCarFollowPath ActiveHostageCarrier { get; private set; }

    void OnEnable()
    {
        if (SceneControl.instance != null
            && SceneControl.instance.currentMission != MissionType.Rescue)
        {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        UnsubscribePath();
        CleanupSpawnedPowerUps();
        if (ActiveHostageCarrier == hostageCarrier)
            ActiveHostageCarrier = null;
        UnhookPlayerDamage();
    }

    void Start()
    {
        enemyInstances.Clear();
        escorts.Clear();
        capturedEscorts = 0;
        roadblockActive = false;
        tunnelSpawned = false;
        pathRegenGraceUntil = 0f;
        followZone = FollowZone.Safe;
        currentPathGap = 0f;
        timePenaltySeconds = 0f;
        farZoneEnterTime = -1f;

        if (gameplayManager == null)
            gameplayManager = FindObjectOfType<GameplayManager>();
        if (enemySpawner == null)
            enemySpawner = FindObjectOfType<EnemySpawner>();
        if (playerCar == null)
            playerCar = FindObjectOfType<PlayerCar>();

        lapsToRoadblock = Random.Range(2, 4); // 2 or 3
        missionTimeLimit = Random.Range(90f, 120f);
        missionStartTime = Time.time;
        timeRemaining = missionTimeLimit;
        alertStopPlayed = false;
        timeOverPlayed = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Sorteia 1 carro da lista (NPCCar2 ou NPCCar3) — nunca os dois
        EnemyCarFollowPath[] toSpawn = PickSingleHostagePrefab();
        enemyInstances.AddRange(enemySpawner.SpawnEnemies(toSpawn));
        SetupSpawnedEnemies();
        HookPlayerDamage();

        if (statusLabelText != null)
        {
            statusLabelText.text = "Status:";
            statusLabelText.color = Color.white;
        }

        UpdateStatusDisplay();
        UpdateProgressDisplay();
        ActivateMissionHuds(true);
        ReduceTrafficForRescue();
        StartCoroutine(EnsureRescuePowerUpSpawning());

        Debug.Log($"[Rescue] Started — timer {missionTimeLimit:F0}s, lapsToRoadblock={lapsToRoadblock}, hostage={(hostageCarrier != null ? hostageCarrier.name : "none")}, escorts={escorts.Count}");
    }

    void Update()
    {
        if (missionEnded)
            return;

        timeRemaining = missionTimeLimit - (Time.time - missionStartTime) - timePenaltySeconds;
        UpdateTimerDisplay();
        TryPlayTimeOverWarning();

        if (timeRemaining <= 0f)
        {
            FailMission("Time limit exceeded");
            return;
        }

        if (hostageCarrier == null || !hostageCarrier.gameObject.activeInHierarchy)
        {
            FailMission("Hostage carrier lost");
            return;
        }

        float elapsed = Time.time - missionStartTime;
        bool inPathGrace = Time.time < pathRegenGraceUntil;
        if (elapsed >= distanceGraceSeconds && !inPathGrace)
            CheckFollowDistance();

        KeepHostageAheadOfPlayer();

        if (!tunnelSpawned)
            TryActivateRoadblock();
        else if (roadblockActive)
            CheckRoadblockWin();

        UpdateProgressDisplay();
        UpdateStatusDisplay();
    }

    /// <summary>
    /// Só recoloca se o sequestrador estiver ATRÁS do player — nunca quando o player
    /// está gerenciando distância de perto (slow powerup).
    /// </summary>
    private void KeepHostageAheadOfPlayer()
    {
        if (hostageCarrier == null || hostageCarrier.player == null)
            return;

        float lead = hostageCarrier.distanceTravelled - hostageCarrier.player.distanceTravelled;
        if (lead < 0f)
            hostageCarrier.distanceTravelled = hostageCarrier.player.distanceTravelled + 35f;
    }

    /// <summary>
    /// Escolhe exatamente 1 prefab da lista de enemies para ser o sequestrador.
    /// </summary>
    private EnemyCarFollowPath[] PickSingleHostagePrefab()
    {
        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogWarning("[Rescue] Nenhum prefab em enemies.");
            return System.Array.Empty<EnemyCarFollowPath>();
        }

        var valid = new List<EnemyCarFollowPath>();
        foreach (EnemyCarFollowPath prefab in enemies)
        {
            if (prefab != null)
                valid.Add(prefab);
        }

        if (valid.Count == 0)
        {
            Debug.LogWarning("[Rescue] Lista enemies só tem nulls.");
            return System.Array.Empty<EnemyCarFollowPath>();
        }

        EnemyCarFollowPath chosen = valid[Random.Range(0, valid.Count)];
        Debug.Log($"[Rescue] Hostage prefab: {chosen.name}");
        return new[] { chosen };
    }

    private void SetupSpawnedEnemies()
    {
        for (int i = 0; i < enemyInstances.Count; i++)
        {
            EnemyCarFollowPath enemy = enemyInstances[i];
            if (enemy == null)
                continue;

            if (enemy.damage == null)
                enemy.damage = enemy.GetComponentInChildren<EnemyDamage>();

            bool isHostage = i == 0
                || (enemy.damage != null && enemy.damage.isHostageCarrier)
                || enemy.GetComponent<HostageMarker>() != null;

            if (isHostage && hostageCarrier == null)
                ConfigureHostageCarrier(enemy);
            else
                ConfigureEscort(enemy);
        }

        if (hostageCarrier == null && enemyInstances.Count > 0)
            ConfigureHostageCarrier(enemyInstances[0]);
    }

    private void ConfigureHostageCarrier(EnemyCarFollowPath enemy)
    {
        hostageCarrier = enemy;
        ActiveHostageCarrier = enemy;
        enemy.ConfigureHostagePacing();

        if (enemy.GetComponent<HostageMarker>() == null)
            enemy.gameObject.AddComponent<HostageMarker>();

        if (enemy.damage != null)
        {
            enemy.damage.isHostageCarrier = true;
            enemy.damage.onDamage += OnHostageHit;
            enemy.damage.onDie = null;
        }
    }

    private void ConfigureEscort(EnemyCarFollowPath enemy)
    {
        escorts.Add(enemy);

        if (enemy.damage == null)
            return;

        enemy.damage.isHostageCarrier = false;
        enemy.damage.onDie = () =>
        {
            if (missionEnded)
                return;

            capturedEscorts++;
            enemy.gameObject.SetActive(false);
            UpdateStatusDisplay();
            Debug.Log($"[Rescue] Escort captured {capturedEscorts}/{escorts.Count}");
        };
    }

    private void HookPlayerDamage()
    {
        if (playerCar == null || playerCar.carDamage == null)
            return;

        playerCar.carDamage.onDamage += OnPlayerTrafficHit;
    }

    private void UnhookPlayerDamage()
    {
        if (playerCar == null || playerCar.carDamage == null)
            return;

        playerCar.carDamage.onDamage -= OnPlayerTrafficHit;
    }

    private void OnPlayerTrafficHit()
    {
        if (missionEnded)
            return;

        timePenaltySeconds += trafficHitTimePenalty;
        if (hostageCarrier != null)
            hostageCarrier.ApplyPanicNudge();

        Debug.Log($"[Rescue] Traffic hit — time -{trafficHitTimePenalty}s, panic nudge");
    }

    private void OnHostageHit()
    {
        if (missionEnded)
            return;

        FailMission("Hit hostage carrier — risk to hostage");
    }

    private void CheckFollowDistance()
    {
        if (hostageCarrier == null || hostageCarrier.player == null)
            return;

        float lead = hostageCarrier.distanceTravelled - hostageCarrier.player.distanceTravelled;
        currentPathGap = Mathf.Abs(lead);

        float worldDist = Vector3.Distance(
            hostageCarrier.transform.position,
            hostageCarrier.player.transform.position);

        bool isClose = currentPathGap <= closeFollowDistance || worldDist <= closeWorldDistance;

        // No clímax do bloqueio: só avisa, não falha
        if (roadblockActive)
        {
            if (isClose)
                followZone = FollowZone.Close;
            else if (currentPathGap > riskFollowDistance)
                followZone = FollowZone.Warning;
            else
                followZone = FollowZone.Safe;
            farZoneEnterTime = -1f;
            TryPlayEscapeAlert();
            return;
        }

        if (currentPathGap > maxFollowDistance)
        {
            followZone = FollowZone.Lost;
            TryPlayEscapeAlert();
            if (farZoneEnterTime < 0f)
                farZoneEnterTime = Time.time;

            if (Time.time - farZoneEnterTime >= farFailDelay)
                FailMission($"Lost hostage — distance {currentPathGap:F1} > {maxFollowDistance}");
            return;
        }

        farZoneEnterTime = -1f;

        // Perto do NPC → FAR (risco de bater no refém)
        if (isClose)
            followZone = FollowZone.Close;
        else if (currentPathGap > riskFollowDistance)
            followZone = FollowZone.Warning;
        else
            followZone = FollowZone.Safe;

        TryPlayEscapeAlert();
    }

    private void TryPlayEscapeAlert()
    {
        if (alertStopPlayed)
            return;
        if (followZone != FollowZone.Warning && followZone != FollowZone.Lost)
            return;

        alertStopPlayed = true;
        if (audioSource != null && alertStopClip != null)
            audioSource.PlayOneShot(alertStopClip);
    }

    private void TryPlayTimeOverWarning()
    {
        if (timeOverPlayed || timeRemaining > TimeWarningThreshold)
            return;

        timeOverPlayed = true;
        if (audioSource != null && timeOverClip != null)
            audioSource.PlayOneShot(timeOverClip);
    }

    private void TryActivateRoadblock()
    {
        if (gameplayManager == null || tunnelSpawned)
            return;

        // Igual Fast Response: polícia na última volta
        if (gameplayManager.currentLap < lapsToRoadblock - 1)
            return;

        if (ActivatePoliceTunnel())
        {
            tunnelSpawned = true;
            roadblockActive = true;
            Debug.Log($"[Rescue] Police roadblock activated on lap {gameplayManager.currentLap}/{lapsToRoadblock}");
        }
    }

    /// <summary>
    /// Ativa os carros de polícia no túnel final — mesmo fluxo da Fast Response.
    /// </summary>
    private bool ActivatePoliceTunnel()
    {
        PathGenerator pathGenerator = PathGenerator.instance;
        if (pathGenerator == null || pathGenerator.StartTunnel == null)
        {
            Debug.LogWarning("[Rescue] StartTunnel não encontrado");
            return false;
        }

        PoliceTunnel tunnel = pathGenerator.StartTunnel.GetComponent<PoliceTunnel>();
        if (tunnel == null)
            tunnel = pathGenerator.StartTunnel.GetComponentInChildren<PoliceTunnel>(true);

        if (tunnel == null)
        {
            Debug.LogWarning("[Rescue] PoliceTunnel component not found in StartTunnel");
            return false;
        }

        tunnel.ActivatePolice();
        Debug.Log("[Rescue] Police activated in final tunnel");
        return true;
    }

    private bool IsNearTunnel(Transform target, float worldRadius)
    {
        PathGenerator pathGenerator = PathGenerator.instance;
        if (pathGenerator == null || pathGenerator.StartTunnel == null || target == null)
            return false;

        float worldDist = Vector3.Distance(target.position, pathGenerator.StartTunnel.transform.position);
        if (worldDist <= worldRadius)
            return true;

        if (pathGenerator.pathCreatorInstance == null || pathGenerator.pathCreatorInstance.path == null)
            return false;

        float pathLen = pathGenerator.pathCreatorInstance.path.length;
        if (pathLen <= 1f)
            return false;

        // Player/NPC perto do fim do path
        CarFollowPath follow = target.GetComponentInParent<CarFollowPath>();
        EnemyCarFollowPath enemy = target.GetComponentInParent<EnemyCarFollowPath>();
        float distTravelled = follow != null ? follow.distanceTravelled
            : enemy != null ? enemy.distanceTravelled : -1f;

        if (distTravelled < 0f)
            return false;

        float localDist = distTravelled % pathLen;
        return localDist >= pathLen - tunnelWinPathDistance;
    }

    private void CheckRoadblockWin()
    {
        if (hostageCarrier == null)
            return;

        // Vitória se o sequestrador chega no bloqueio
        if (IsNearTunnel(hostageCarrier.transform, tunnelWinRadius))
        {
            WinMission();
            return;
        }

        // Ou se o player chega no túnel ainda acompanhando (não Lost)
        if (playerCar != null
            && playerCar.Follow != null
            && IsNearTunnel(playerCar.Follow.transform, tunnelWinRadius)
            && followZone != FollowZone.Lost)
        {
            WinMission();
        }
    }

    private void WinMission()
    {
        if (missionEnded)
            return;

        missionEnded = true;
        OnMissionEnd();
        Debug.Log("[Rescue] Hostage secured at roadblock!");
        if (gameplayManager != null)
            gameplayManager.EndGameplay(true);
    }

    private void FailMission(string reason)
    {
        if (missionEnded)
            return;

        missionEnded = true;
        Debug.Log($"[Rescue] Failed — {reason}");
        OnMissionEnd();
        if (gameplayManager != null)
            gameplayManager.EndGameplay(false);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        float display = Mathf.Max(0f, timeRemaining);
        int minutes = (int)(display / 60f);
        int seconds = (int)(display % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
        timerText.color = display <= TimeWarningThreshold ? Color.red : Color.white;
    }

    private void UpdateProgressDisplay()
    {
        if (progressPercentageText == null || lapsToRoadblock <= 0 || gameplayManager == null)
            return;

        float progress = (gameplayManager.currentLap / lapsToRoadblock) * 100f;
        progress = Mathf.Clamp(progress, 0f, 100f);
        progressPercentageText.text = $"{progress:F0}%";
    }

    private void UpdateStatusDisplay()
    {
        string zone;
        Color color = Color.white;

        if (roadblockActive && followZone != FollowZone.Close && followZone != FollowZone.Warning)
        {
            zone = "ROADBLOCK";
            color = new Color(0.35f, 1f, 0.45f);
        }
        else if (followZone == FollowZone.Lost)
        {
            zone = "FAR!";
            color = Color.red;
        }
        else if (followZone == FollowZone.Warning)
        {
            zone = "RISK";
            color = new Color(1f, 0.55f, 0.1f);
        }
        else if (followZone == FollowZone.Close)
        {
            zone = "FAR";
            color = new Color(1f, 0.85f, 0.2f);
        }
        else if (roadblockActive)
        {
            zone = "ROADBLOCK";
            color = new Color(0.35f, 1f, 0.45f);
        }
        else
        {
            zone = "SAFE";
            color = Color.white;
        }

        // Mesmo padrão do Time: label fixo à esquerda, valor à direita
        if (statusLabelText != null)
        {
            statusLabelText.text = "Status:";
            statusLabelText.color = Color.white;
        }

        if (statusText != null)
        {
            statusText.text = zone;
            statusText.color = color;
        }
    }

    private void ReduceTrafficForRescue()
    {
        TrafficSpawner trafficSpawner = FindObjectOfType<TrafficSpawner>();
        if (trafficSpawner == null)
            return;

        // ~45% a menos de trânsito — missão é de gerenciamento, não de batida
        int reduced = Mathf.Max(5, Mathf.RoundToInt(trafficSpawner.amountSpawn * 0.55f));
        Debug.Log($"[Rescue] Traffic reduzido: {trafficSpawner.amountSpawn} -> {reduced}");
        trafficSpawner.amountSpawn = reduced;
    }

    private void ActivateMissionHuds(bool activate)
    {
        if (missionHudVisuals == null)
            return;

        foreach (GameObject hudVisual in missionHudVisuals)
        {
            if (hudVisual != null)
                hudVisual.SetActive(activate);
        }
    }

    // --- Powerups ---

    private IEnumerator EnsureRescuePowerUpSpawning()
    {
        if (rescueNpcBoostPrefab == null && rescueNpcSlowPrefab == null)
        {
            Debug.LogWarning("[Rescue] Nenhum prefab de NPC powerup configurado!");
            // Still enable global spawner for Health/Shield
            ConfigureGlobalSpawnerForRescue();
            yield break;
        }

        yield return new WaitUntil(() =>
            PathGenerator.instance != null
            && PathGenerator.instance.pathCreatorInstance != null
            && PathGenerator.instance.pathCreatorInstance.path != null
            && PathGenerator.instance.pathCreatorInstance.path.length > 10f);

        if (missionEnded)
            yield break;

        ConfigureGlobalSpawnerForRescue();

        if (powerUpHolder == null)
        {
            var holderGo = new GameObject("RescuePowerUpHolder");
            powerUpHolder = holderGo.transform;
        }

        PathCreator pathCreator = PathGenerator.instance.pathCreatorInstance;
        SubscribePath(pathCreator);
        SpawnPowerUpsAlongPath();
    }

    private void ConfigureGlobalSpawnerForRescue()
    {
        MultipleObjectSpawner globalSpawner = FindObjectOfType<MultipleObjectSpawner>();
        if (globalSpawner == null)
            return;

        // Remove only cumulative SpeedPowerUp; keep Health/Shield
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
        if (subscribedPathCreator == pathCreator)
            return;

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
        if (missionEnded)
            return;

        // Path/túnel mudou: sincroniza NPC e evita fail falso por distância
        pathRegenGraceUntil = Time.time + 3f;
        if (hostageCarrier != null && hostageCarrier.player != null)
            hostageCarrier.distanceTravelled = hostageCarrier.player.distanceTravelled + 40f;

        farZoneEnterTime = -1f;
        if (followZone == FollowZone.Lost)
            followZone = FollowZone.Safe;

        // Reaplica polícia no túnel reposicionado (igual Fast Response no fim)
        if (tunnelSpawned)
            ActivatePoliceTunnel();

        SpawnPowerUpsAlongPath();
    }

    private void SpawnPowerUpsAlongPath()
    {
        CleanupSpawnedPowerUps();

        if (rescueNpcBoostPrefab == null || rescueNpcSlowPrefab == null)
        {
            Debug.LogWarning("[Rescue] Precisa dos dois prefabs (boost + slow) para spawn em pares.");
            return;
        }

        PathGenerator pathGenerator = PathGenerator.instance;
        if (pathGenerator == null || pathGenerator.pathCreatorInstance == null)
            return;

        VertexPath path = pathGenerator.pathCreatorInstance.path;
        if (path == null || path.length <= 10f)
            return;

        int pairCount = Random.Range(minPowerUpPairs, maxPowerUpPairs + 1);
        float minDist = path.length * 0.18f;
        float maxDist = path.length * 0.88f;
        float span = Mathf.Max(maxDist - minDist, 1f);
        // Espaça bem os pares ao longo do path (não a cada segundo)
        float step = span / (pairCount + 1);

        // Começa com um lado aleatório; pares seguintes tendem a alternar
        bool boostOnLeft = Random.value < 0.5f;

        for (int i = 0; i < pairCount; i++)
        {
            float dst = minDist + step * (i + 1);
            // Pequena variação pra não ficar em linha reta perfeita
            dst += Random.Range(-step * 0.15f, step * 0.15f);
            dst = Mathf.Clamp(dst, minDist, maxDist);

            // ~65% de chance de alternar lado a cada par
            if (i > 0 && Random.value < 0.65f)
                boostOnLeft = !boostOnLeft;

            GameObject leftPrefab = boostOnLeft ? rescueNpcBoostPrefab : rescueNpcSlowPrefab;
            GameObject rightPrefab = boostOnLeft ? rescueNpcSlowPrefab : rescueNpcBoostPrefab;

            SpawnPowerUpAt(path, dst, -sideOffset, leftPrefab);
            SpawnPowerUpAt(path, dst, sideOffset, rightPrefab);
        }

        Debug.Log($"[Rescue] NPC powerup pairs on path: {pairCount} ({spawnedPowerUps.Count} pickups)");
    }

    private void SpawnPowerUpAt(VertexPath path, float distance, float lateralOffset, GameObject prefab)
    {
        if (prefab == null)
            return;

        Vector3 point = path.GetPointAtDistance(distance, EndOfPathInstruction.Loop);
        Quaternion rot = path.GetRotationAtDistance(distance, EndOfPathInstruction.Loop);
        Vector3 normal = path.GetNormalAtDistance(distance, EndOfPathInstruction.Loop);
        point += normal * lateralOffset;
        point.y += heightOffset;

        Quaternion rotation = rot * Quaternion.Euler(0f, 0f, 90f);
        Transform parent = powerUpHolder != null ? powerUpHolder : null;
        GameObject powerUp = Instantiate(prefab, point, rotation, parent);
        spawnedPowerUps.Add(powerUp);
    }

    private void CleanupSpawnedPowerUps()
    {
        foreach (GameObject powerUp in spawnedPowerUps)
        {
            if (powerUp != null)
                Destroy(powerUp);
        }
        spawnedPowerUps.Clear();
    }

    private void OnMissionEnd()
    {
        if (hostageCarrier != null && hostageCarrier.damage != null)
            hostageCarrier.damage.onDamage -= OnHostageHit;

        UnhookPlayerDamage();
        UnsubscribePath();
        ActivateMissionHuds(false);
        CleanupSpawnedPowerUps();

        if (powerUpHolder != null)
            Destroy(powerUpHolder.gameObject);
        powerUpHolder = null;

        if (ActiveHostageCarrier == hostageCarrier)
            ActiveHostageCarrier = null;

        if (statusLabelText != null)
            statusLabelText.text = "Total Enemy:";
    }
}
