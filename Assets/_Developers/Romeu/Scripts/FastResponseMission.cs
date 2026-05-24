using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FastResponseMission : MonoBehaviour
{
    public float lapsToWin = 4;
    public GameplayManager gameplayManager;
    public PlayerCar playerCar;
    public AudioClip damageAudio;
    private AudioSource audioSource;

    // UI References
    [SerializeField] private TextMeshProUGUI progressPercentageText;
    [SerializeField] private TextMeshProUGUI hitsDisplayText;
    [SerializeField] private GameObject[] missionHudVisuals;

    public int damageTaken = 0;
    public bool tunnelSpawned = false;
    public bool completed = false;

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

        // Registrar callback de dano - incrementa contador de batidas
        playerCar.carDamage.onDamage = () =>
        {
            damageTaken++;
            UpdateHitsDisplay();
            Debug.Log($"Traffic hit! Count: {damageTaken}/{maxHitsAllowed}");

            // Reproduzir som ao 4º hit
            if(damageTaken == 4)
            {
                audioSource.PlayOneShot(damageAudio);
            }
        };

        // Atualizar UI de hits inicial
        UpdateHitsDisplay();

        // Ativar HUDs específicos da missão
        ActivateMissionHuds(true);
    }

    void Update()
    {
        // Atualizar UI de progresso em tempo real
        UpdateProgressDisplay();

        // Verificar limite de hits (mission fail)
        if(damageTaken > maxHitsAllowed && !completed)
        {
            Debug.Log($"Mission failed - Hit limit exceeded: {damageTaken}/{maxHitsAllowed}");
            OnMissionEnd();
            gameplayManager.EndGameplay(false);
            completed = true;
            return;
        }

        // Verificar se completou todas as laps
        if(gameplayManager.currentLap >= lapsToWin && !completed)
        {
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
            hitsDisplayText.text = $"Hits: {damageTaken}/{maxHitsAllowed}";
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
    }
}
