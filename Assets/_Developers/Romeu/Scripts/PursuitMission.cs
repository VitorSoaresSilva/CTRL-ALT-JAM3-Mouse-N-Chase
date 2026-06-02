using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    // UI References
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI enemiesStatusText; // Mostra: X presos / Y total
    [SerializeField] private GameObject[] missionHudVisuals;

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

        Debug.Log($"PursuitMission started - Time limit: {missionTimeLimit:F1}s");

        enemyInstances.AddRange(enemySpawner.SpawnRandomEnemies(enemies));

        foreach (EnemyCarFollowPath enemy in enemyInstances)
        {
            enemy.damage.onDie = () =>
            {
                // Ignorar se missão já terminou
                if (missionEnded)
                    return;

                destroyedEnemies++;
                enemy.gameObject.SetActive(false);
                UpdateEnemiesStatusDisplay();

                Debug.Log($"Enemy captured! {destroyedEnemies}/{enemies.Length}");

                if (destroyedEnemies >= enemies.Length)
                {
                    missionEnded = true;
                    OnMissionEnd();
                    gameplayManager.EndGameplay(true);
                }
            };
        }

        // Atualizar UI inicial
        UpdateEnemiesStatusDisplay();
        ActivateMissionHuds(true);
    }

    void Update()
    {
        // Early return se a missão já terminou
        if (missionEnded)
            return;

        // Calcular tempo restante
        timeRemaining = missionTimeLimit - (Time.time - missionStartTime);
        UpdateTimerDisplay();

        // Verificar timeout (tempo esgotado)
        if (timeRemaining <= 0)
        {
            missionEnded = true;
            Debug.Log("Mission failed - Time limit exceeded!");
            FailMission();
            return;
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = (int)(timeRemaining / 60f);
            int seconds = (int)(timeRemaining % 60f);
            timerText.text = $"{minutes:D2}:{seconds:D2}";

            // Mudar cor do texto para vermelho se tempo está acabando (menos de 30s)
            if (timeRemaining <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    private void UpdateEnemiesStatusDisplay()
    {
        if (enemiesStatusText != null)
        {
            enemiesStatusText.text = $"{destroyedEnemies}/{enemies.Length}";
        }
    }

    private void FailMission()
    {
        // Parar todas as coroutines ativas
        StopAllCoroutines();

        // Limpar recursos da missão
        OnMissionEnd();

        // Notificar falha ao gameplay
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
        // Desativar HUDs específicos da missão
        ActivateMissionHuds(false);
    }
}
