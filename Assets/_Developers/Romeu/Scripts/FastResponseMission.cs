using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using PathCreation;

public class FastResponseMission : MonoBehaviour
{
    public float lapsToWin = 4;
    public bool allowChaoticTraffic = true; // Flag para permitir trânsito caótico
    public GameplayManager gameplayManager;
    public PlayerCar playerCar;
    [FormerlySerializedAs("damageAudio")]
    [SerializeField] private AudioClip timeOverClip;
    private AudioSource audioSource;
    private bool timeOverPlayed;

    // UI References
    [SerializeField] private TextMeshProUGUI progressPercentageText;
    [SerializeField] private TextMeshProUGUI hitsDisplayText;
    [SerializeField] private TextMeshProUGUI speedPowerUpCounterText; // Novo: contador de SpeedPowerUps
    [SerializeField] private GameObject[] missionHudVisuals;

    // SpeedPowerUp obrigatório
    [SerializeField] private GameObject speedPowerUpPrefab;
    [SerializeField] private MultipleObjectSpawner speedPowerUpSpawner; // Usar spawner ao invés de spawn direto
    [SerializeField] private int minSpeedPowerUps = 2;
    [SerializeField] private int maxSpeedPowerUps = 4;
    [SerializeField] private Transform[] speedPowerUpSpawnPoints; // Pontos customizados (opcional)
    private bool speedPowerUpCollected = false;
    private List<GameObject> spawnedSpeedPowerUps = new List<GameObject>();

    public int damageTaken = 0;
    public bool tunnelSpawned = false;
    public bool completed = false;

    // Proteção contra múltiplas chamadas de falha
    private bool missionEnded = false;

    /// <summary>
    /// Propriedade pública para outras classes verificarem se a missão já terminou
    /// </summary>
    public bool IsMissionEnded => missionEnded;

    private int maxHitsAllowed = 0;
    
    void OnEnable()
    {
        if(SceneControl.instance != null)
        {
            if(SceneControl.instance.currentMission != MissionType.FastResponse)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void Start()
    {
        if(gameplayManager == null)
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }
        if(playerCar == null) playerCar = FindObjectOfType<PlayerCar>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        timeOverPlayed = false;

        // Randomizar lapsToWin: 2-4 comum, 5 raro
        int randomLaps = Random.Range(1, 100);
        if(randomLaps <= 15) // 15% de chance para 5 laps
        {
            lapsToWin = 5;
        }
        else // 85% para 2-4 laps
        {
            lapsToWin = Random.Range(2, 5);
        }

        // Calcular limite de hits: lapsToWin + 1
        maxHitsAllowed = (int)lapsToWin + 1;

        // Log para debug
        Debug.Log($"FastResponseMission started - Laps to win: {lapsToWin}, Max hits allowed: {maxHitsAllowed}");

        // Contador estático persiste entre reloads — zera ao começar a fase
        SpeedPowerUp.ResetSpeedPowerUpCounter();
        speedPowerUpCollected = false;

        // Registrar callback de dano - incrementa contador de batidas
        playerCar.carDamage.onDamage = () =>
        {
            // Ignorar danos se a missão já terminou
            if (missionEnded)
                return;

            damageTaken++;
            UpdateHitsDisplay();
            Debug.Log($"Traffic hit! Count: {damageTaken}/{maxHitsAllowed}");

            // Alerta crítico: 1 hit antes do fail (fail quando damageTaken > maxHitsAllowed)
            if (!timeOverPlayed && damageTaken >= maxHitsAllowed
                && audioSource != null && timeOverClip != null)
            {
                timeOverPlayed = true;
                audioSource.PlayOneShot(timeOverClip);
            }
        };

        // Atualizar UI de hits inicial
        UpdateHitsDisplay();
        UpdateSpeedPowerUpCounter();

        // Ativar HUDs específicos da missão
        ActivateMissionHuds(true);

        // Garantir spawn obrigatório do SpeedPowerUp via MultipleObjectSpawner
        EnsureSpeedPowerUpSpawning();
    }

    private void EnsureSpeedPowerUpSpawning()
    {
        if (speedPowerUpPrefab == null)
        {
            Debug.LogWarning("SpeedPowerUp prefab não foi configurado na FastResponseMission!");
            return;
        }

        // Procurar PathGenerator para obter pontos do path
        PathGenerator pathGenerator = PathGenerator.instance;
        if (pathGenerator == null || pathGenerator.pathCreatorInstance == null)
        {
            Debug.LogWarning("PathGenerator não encontrado!");
            return;
        }

        VertexPath path = pathGenerator.pathCreatorInstance.path;
        if (path == null || path.length <= 0)
        {
            Debug.LogWarning("Path não está pronto!");
            return;
        }

        // Determinar quantidade de powerups a spawnar
        int powerUpCount = Random.Range(minSpeedPowerUps, maxSpeedPowerUps + 1);

        // Se existem spawn points customizados, usar eles
        if (speedPowerUpSpawnPoints != null && speedPowerUpSpawnPoints.Length > 0)
        {
            for (int i = 0; i < powerUpCount && i < speedPowerUpSpawnPoints.Length; i++)
            {
                if (speedPowerUpSpawnPoints[i] != null)
                {
                    GameObject powerUp = Instantiate(speedPowerUpPrefab, 
                        speedPowerUpSpawnPoints[i].position, 
                        Quaternion.identity);
                    spawnedSpeedPowerUps.Add(powerUp);
                }
            }
            Debug.Log($"SpeedPowerUps spawados em {powerUpCount} pontos customizados");
        }
        else
        {
            // Caso contrário, spawnar em pontos aleatórios do path
            for (int i = 0; i < powerUpCount; i++)
            {
                // Gerar uma distância aleatória ao longo do path
                float randomDistance = Random.Range(path.length * 0.2f, path.length * 0.9f);
                Vector3 spawnPos = path.GetPointAtDistance(randomDistance, EndOfPathInstruction.Loop);

                // Adicionar um offset lateral pequeno para não ficar exatamente na rota
                Vector3 normalAtDistance = path.GetNormalAtDistance(randomDistance, EndOfPathInstruction.Loop);
                float lateralOffset = Random.Range(-3f, 3f);
                spawnPos += normalAtDistance * lateralOffset;

                GameObject powerUp = Instantiate(speedPowerUpPrefab, spawnPos, Quaternion.identity);
                spawnedSpeedPowerUps.Add(powerUp);
            }
            Debug.Log($"SpeedPowerUps spawados em {powerUpCount} pontos aleatórios do path");
        }

        // Procurar MultipleObjectSpawner se não estiver configurado (fallback)
        if (speedPowerUpSpawner == null)
        {
            speedPowerUpSpawner = FindObjectOfType<MultipleObjectSpawner>();
        }

        if (speedPowerUpSpawner != null)
        {
            speedPowerUpSpawner.gameObject.SetActive(true);
            Debug.Log("MultipleObjectSpawner ativado para spawnar outros objetos");
        }
    }

    void Update()
    {
        // Early return se a missão já terminou
        if (missionEnded)
            return;

        // Atualizar UI de progresso em tempo real
        UpdateProgressDisplay();

        // Atualizar contador de SpeedPowerUps coletados
        UpdateSpeedPowerUpCounter();

        // Verificar limite de hits (mission fail)
        if(damageTaken > maxHitsAllowed)
        {
            missionEnded = true;
            Debug.Log($"Mission failed - Hit limit exceeded: {damageTaken}/{maxHitsAllowed}");
            FailMission();
            return;
        }

        // Verificar se completou todas as laps
        if(gameplayManager.currentLap >= lapsToWin)
        {
            missionEnded = true;
            //Debug.Log("Mission Complete");
            OnMissionEnd();
            gameplayManager.EndGameplay(true);
            completed = true;
        }

        // Ativar polícia no tunnel na última lap
        if (gameplayManager.currentLap >= lapsToWin - 1 && !tunnelSpawned)
        {
            ActivatePoliceTunnel();
            Debug.Log("Police tunnel activated");
            tunnelSpawned = true;
        }
    }

    private void FailMission()
    {
        // Remover callback de dano para evitar múltiplas chamadas
        if (playerCar != null && playerCar.carDamage != null)
        {
            playerCar.carDamage.onDamage = null;
        }

        // Parar todas as coroutines ativas
        StopAllCoroutines();

        // Limpar recursos da missão
        OnMissionEnd();

        // Notificar falha ao gameplay
        if (gameplayManager != null)
        {
            gameplayManager.EndGameplay(false);
        }

        completed = true;
    }

    private void UpdateProgressDisplay()
    {
        if(progressPercentageText != null && lapsToWin > 0)
        {
            float progress = (gameplayManager.currentLap / lapsToWin) * 100f;
            progress = Mathf.Clamp(progress, 0, 100);
            progressPercentageText.text = $"{progress:F0}%";
        }
    }

    private void UpdateHitsDisplay()
    {
        if(hitsDisplayText != null)
        {
            hitsDisplayText.text = $"{damageTaken}/{maxHitsAllowed}";
        }
    }

    private void ActivatePoliceTunnel()
    {
        // Obter o último tunnel do PathGenerator (startTunnel = tunnel no final do caminho)
        PathGenerator pathGenerator = PathGenerator.instance;
        if (pathGenerator == null || pathGenerator.StartTunnel == null) return;

        // Procurar o script PoliceTunnel no último tunnel
        PoliceTunnel tunnelPoliceTunnel = pathGenerator.StartTunnel.GetComponent<PoliceTunnel>();
        if (tunnelPoliceTunnel != null)
        {
            tunnelPoliceTunnel.ActivatePolice();
            Debug.Log("Police activated in final tunnel");
        }
        else
        {
            Debug.LogWarning("PoliceTunnel component not found in StartTunnel");
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
        // Desativar HUDs específicos da missão
        ActivateMissionHuds(false);

        // Limpar SpeedPowerUps spawados
        foreach (GameObject powerUp in spawnedSpeedPowerUps)
        {
            if (powerUp != null)
            {
                Destroy(powerUp);
            }
        }
        spawnedSpeedPowerUps.Clear();

        // Resetar contador de SpeedPowerUps para próxima missão
        SpeedPowerUp.ResetSpeedPowerUpCounter();
    }

    private void UpdateSpeedPowerUpCounter()
    {
        if(speedPowerUpCounterText != null)
        {
            int collectedCount = SpeedPowerUp.GetSpeedPowerUpCollectCount();
            speedPowerUpCounterText.text = collectedCount.ToString();
        }
    }
}
