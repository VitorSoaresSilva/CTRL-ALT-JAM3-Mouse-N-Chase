using PathCreation;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MainMenu;
using _Developers.Vitor;

public class GameplayManager : MonoBehaviour
{
    [Header("Player")] public PlayerCar playerCar;
    [Header("Player")] public PlayerCar secretCar;
    [SerializeField] private CameraControl cameraControl;

    [SerializeField, Header("Start")] private float SceneCameraSpeed = 2f;
    [SerializeField] private AudioSource RadioAudio;
    [SerializeField] private AudioSource PlayerResponse;

    [SerializeField, Header("Canvas")] private Canvas GameCanvas;
    [SerializeField] private Slider HealthSlider;
    [SerializeField] private GameObject FailPanel;
    [SerializeField] private GameObject SucceedPanel;
    [SerializeField] private TextMeshProUGUI pointsValueText;

    [SerializeField, Header("Upgrades")] private Image shieldSlot;
    [SerializeField] private Image slotSlot;
    [SerializeField] private Image bumperSlot;

    [SerializeField, Header("Path")] private PathCreator pathCreator;
    [SerializeField] private int maxLaps = 10; // maximo de voltas

    [SerializeField, Header("Chaotic Traffic")] private bool enableChaoticTraffic = true;
    [SerializeField] private float chaoticTrafficChance = 0.5f;

    private float StartSceneTime = 5;
    public float lapsToFail = 5;
    public float currentLap = 0;
    public bool isIntroPlaying = false;
    public bool isChaoticTraffic { get; private set; } = false;

    // Prote��o contra GameOver m�ltiplo
    private bool gameplayEnded = false;
    public bool HasGameplayEnded => gameplayEnded;

    // Serialized default car; playerCar may be swapped to secretCar at runtime.
    private PlayerCar defaultCar;

    void OnEnable()
    {
        if(SceneControl.instance != null) SceneControl.instance.AddGameplayManager(this);
        if(GameCanvas == null) GameCanvas = GetComponent<Canvas>();
        if(HealthSlider == null) HealthSlider = GameCanvas.GetComponentInChildren<Slider>(true);
        if(cameraControl == null) cameraControl = FindObjectOfType<CameraControl>();
        if(playerCar == null) playerCar = FindObjectOfType<PlayerCar>();

        if (defaultCar == null)
            defaultCar = playerCar;

        if(secretCar != null && CareerPoints.instance != null)
        {
            // Valida��o: carro secreto s� pode ser usado se liberado E selecionado no menu
            if(CareerPoints.instance.SecretCarUnlocked && CareerPoints.instance.usingSecretCar)
            {
                playerCar = secretCar;
                cameraControl = secretCar.GetComponentInChildren<CameraControl>(true);
            }
            else
            {
                // Se n�o est� liberado ou n�o foi selecionado, sempre usa o carro padr�o
                CareerPoints.instance.usingSecretCar = false;
                if (defaultCar != null)
                    playerCar = defaultCar;
            }
        }

        EnsureSinglePlayerCarActive(activateSelected: false);

        if(CareerPoints.instance != null)
        {
            if (shieldSlot != null)
            {
                //Debug.Log(CareerPoints.instance.ShieldUnlocked);
                shieldSlot.gameObject.SetActive(CareerPoints.instance.ShieldUnlocked);
            }

            if (slotSlot != null)
            {
                //Debug.Log(CareerPoints.instance.SlotUnlocked);
                slotSlot.gameObject.SetActive(CareerPoints.instance.SlotUnlocked);
            }

            if (bumperSlot != null)
            {
                //Debug.Log(CareerPoints.instance.BumperUnlocked);
                bumperSlot.gameObject.SetActive(CareerPoints.instance.BumperUnlocked);
            }
        }
    }

    void OnDisable()
    {
        Debug.Log("[GameplayManager] OnDisable - Limpando antes de descarregar cena");

        // Resetar flag de gameplay
        gameplayEnded = false;

        // Parar coroutines ativas do GameplayManager
        StopAllCoroutines();

        // Limpar referencia em SceneControl
        if (SceneControl.instance != null)
        {
            SceneControl.instance.AddGameplayManager(null);
        }


        if (playerCar != null && playerCar.carDamage != null)
        {
            playerCar.carDamage.onDamage = null;
        }
    }

    private void Start()
    {
        lapsToFail = UnityEngine.Random.Range(5, maxLaps);
    }

    /// <summary>
    /// Keeps exactly one player car active so only one AudioListener is in the scene.
    /// </summary>
    void EnsureSinglePlayerCarActive(bool activateSelected)
    {
        if (!activateSelected)
        {
            // During load: only keep the unused secret car off. Leave the default car
            // alone so we still have one AudioListener until StartGameplay swaps.
            if (secretCar != null && secretCar != playerCar)
                secretCar.gameObject.SetActive(false);
            return;
        }

        if (defaultCar != null && defaultCar != playerCar)
            defaultCar.gameObject.SetActive(false);
        if (secretCar != null && secretCar != playerCar)
            secretCar.gameObject.SetActive(false);
        if (playerCar != null)
            playerCar.gameObject.SetActive(true);
    }

    public void StartGameplay()
    {
        //Debug.Log("Starting gameplay");
        EnsureSinglePlayerCarActive(activateSelected: true);
        isIntroPlaying = true;

        InitializeChaoticTraffic();

        if(SceneControl.instance != null)
        {
            AudioClip[] clips = new AudioClip[0];
            switch (SceneControl.instance.currentMission)
            {
                case MissionType.FastResponse:
                    clips = SceneControl.instance.FastResponseClips;
                    break;
                case MissionType.Pursuit:
                    clips = SceneControl.instance.PursuitClips;
                    break;
                case MissionType.Rescue:
                    clips = SceneControl.instance.RescueClips;
                    break;
                case MissionType.Boss:
                    clips = SceneControl.instance.BossClips;
                    break;
            }

            if(clips.Length > 0)
            {
                RadioAudio.clip = clips[Random.Range(0, clips.Length)];
                StartSceneTime = RadioAudio.clip.length;
            }
        }

        float followDist = cameraControl.FollowDistance;
        cameraControl.FollowDistance = -followDist;

        float initialSpeed = playerCar.Follow.speed;
        playerCar.Follow.speed = initialSpeed / 2;

        playerCar.carDamage.takeDamage = false;

        // Play start cinematic
        StartCoroutine(PlayStartScene());
        IEnumerator PlayStartScene() // Mudar velocidade do player
        {
            yield return new WaitForSeconds(1f);

            if (RadioAudio != null)
                RadioAudio.Play();

            yield return new WaitForSeconds(StartSceneTime);

            // volta a camera a posi��o original
            playerCar.Siren.activateSiren = true;
            while(Mathf.Abs(cameraControl.FollowDistance - followDist) > 0.2f)
            {
                cameraControl.FollowDistance = Mathf.Lerp(cameraControl.FollowDistance, followDist, SceneCameraSpeed * Time.deltaTime);
                //playerCar.Follow.speed = 20; // lerp 
                playerCar.Follow.speed = Mathf.Lerp(playerCar.Follow.speed, initialSpeed, SceneCameraSpeed * Time.deltaTime);
                yield return null;
            }

            cameraControl.FollowDistance = followDist;
            playerCar.carDamage.takeDamage = true;
            isIntroPlaying = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HealthSlider.value = playerCar.carDamage.health / 100;
        pointsValueText.text = CareerPoints.instance.Points.ToString();

        if (playerCar != null)
        {
            if(CareerPoints.instance.Points <= 0)
            {
                CareerPoints.instance.Save();
                SceneControl.instance.ChangeScene("GameOver");
            }
        }

        if(playerCar.carDamage.health <= 0 || currentLap > lapsToFail)
        {
            EndGameplay(false);
        }
    }

    public void EndGameplay(bool success = false)
    {
        // Prote��o contra m�ltiplas chamadas
        if (gameplayEnded)
        {
            Debug.LogWarning("[GameplayManager] EndGameplay j� foi chamado! Ignorando chamada duplicada.");
            return;
        }

        gameplayEnded = true;
        Debug.Log($"[GameplayManager] EndGameplay iniciado (sucesso: {success})");

        if(FailPanel != null)
            FailPanel.SetActive(!success);
        if(SucceedPanel != null)
            SucceedPanel.SetActive(success);

        playerCar.gameObject.SetActive(false);

        if (CareerPoints.instance != null)
        {
            if(success) CareerPoints.instance.CompleteMission(SceneControl.instance.currentMission);
            else CareerPoints.instance.RemovePoints(250);
        }

        // Limpar recursos ANTES de iniciar a coroutine de saida
        CleanupGameplay();

        // Iniciar saida DEPOIS da limpeza (para nao ser parada por StopAllCoroutines)
        StartCoroutine(ExitGameplay(success));
    }

    private void CleanupGameplay()
    {
        Debug.Log("[GameplayManager] Iniciando limpeza de recursos de gameplay");

        // N�O parar StopAllCoroutines() aqui! Pode parar a coroutine ExitGameplay()
        // que foi iniciada AP�S esta limpeza

        // 1. Parar e limpar TrafficSpawner completamente
        CleanupTrafficSpawner();

        // 3. Destruir todos os carros de tr�fego spawados
        TrafficCarFollowPath[] trafficCars = FindObjectsByType<TrafficCarFollowPath>(FindObjectsSortMode.None);
        foreach (TrafficCarFollowPath trafficCar in trafficCars)
        {
            if (trafficCar != null)
            {
                Destroy(trafficCar.gameObject);
            }
        }
        Debug.Log($"[GameplayManager] {trafficCars.Length} carros de tr�fego destru�dos");

        // 4. Parar todas as miss�es e limpar seus listeners
        FastResponseMission fastResponse = FindObjectOfType<FastResponseMission>();
        if (fastResponse != null)
        {
            fastResponse.StopAllCoroutines();
            // Limpar listeners do carDamage
            if (playerCar != null && playerCar.carDamage != null)
            {
                playerCar.carDamage.onDamage = null;
            }
            Destroy(fastResponse.gameObject);
        }

        PursuitMission pursuit = FindObjectOfType<PursuitMission>();
        if (pursuit != null)
        {
            pursuit.StopAllCoroutines();
            Destroy(pursuit.gameObject);
        }

        RescueMission rescue = FindObjectOfType<RescueMission>();
        if (rescue != null)
        {
            rescue.StopAllCoroutines();
            Destroy(rescue.gameObject);
        }

        BossMission boss = FindObjectOfType<BossMission>();
        if (boss != null)
        {
            boss.StopAllCoroutines();
            Destroy(boss.gameObject);
        }

        Debug.Log("[GameplayManager] Missoes paradas e destruidas");

        // 5. Resetar flags importantes
        isIntroPlaying = false;
        currentLap = 0;

        // 6. Limpar spawners de objetos multiplos
        MultipleObjectSpawner[] spawners = FindObjectsByType<MultipleObjectSpawner>(FindObjectsSortMode.None);
        foreach (MultipleObjectSpawner spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.StopAllCoroutines();
            }
        }

        // 7. Limpar Rigidbodies com velocidades invalidas
        Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb != null && rb.gameObject != null && rb.gameObject != playerCar?.gameObject)
            {
                // Resetar velocidade invalida
                if (!_Developers.Vitor.ValidationUtility.IsValidVelocity(rb.velocity))
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    Debug.LogWarning($"[GameplayManager] Rigidbody com velocidade invalida resetado: {rb.gameObject.name}");
                }
            }
        }

        // 8. Resetar estado do PlayerCar se necessario
        if (playerCar != null && playerCar.carDamage != null)
        {
            // Limpar todos os listeners
            playerCar.carDamage.onDamage = null;
        }

        // 9. Resetar TimeScale caso esteja alterado
        if (Time.timeScale != 1f)
        {
            Debug.LogWarning($"[GameplayManager] Time.timeScale estava {Time.timeScale}, resetando para 1");
            Time.timeScale = 1f;
        }

        // 10. Parar PathGenerator se ainda estiver gerando
        CleanupPathGenerator();

        // 11. Cancelar todos os Invokes pendentes em GameplayManager
        CancelInvoke();

        // 12. Resetar velocidades de Rigidbodies v�lidos (mantendo apenas safe values)
        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb != null && rb.gameObject != null && rb.gameObject.CompareTag("Player") == false)
            {
                // Para objetos n�o-player, setar velocidades baixas para evitar f�sica pesada
                if (rb.velocity.magnitude > 5f)
                {
                    rb.velocity = rb.velocity * 0.1f; // Reduzir drasticamente
                    rb.angularVelocity = rb.angularVelocity * 0.1f;
                }
            }
        }

        // 13. Limpar todos os listeners de eventos
        CleanupListeners();

        Debug.Log("[GameplayManager] Limpeza de recursos completa");
    }

    private void InitializeChaoticTraffic()
    {
        if (!enableChaoticTraffic)
        {
            isChaoticTraffic = false;
            Debug.Log("Tr�nsito ca�tico desativado globalmente");
            return;
        }

        bool missionAllowsChaotic = true;

        FastResponseMission fastResponseMission = FindObjectOfType<FastResponseMission>();
        if (fastResponseMission != null)
        {
            missionAllowsChaotic = fastResponseMission.allowChaoticTraffic;
        }
        else
        {
            PursuitMission pursuitMission = FindObjectOfType<PursuitMission>();
            if (pursuitMission != null)
            {
                missionAllowsChaotic = pursuitMission.allowChaoticTraffic;
            }
            else
            {
                RescueMission rescueMission = FindObjectOfType<RescueMission>();
                if (rescueMission != null)
                {
                    missionAllowsChaotic = rescueMission.allowChaoticTraffic;
                }
                else
                {
                    BossMission bossMission = FindObjectOfType<BossMission>();
                    if (bossMission != null)
                    {
                        missionAllowsChaotic = bossMission.allowChaoticTraffic;
                    }
                }
            }
        }

        if (!missionAllowsChaotic)
        {
            isChaoticTraffic = false;
            Debug.Log("Tr�nsito ca�tico bloqueado por esta miss�o");
            return;
        }

        isChaoticTraffic = Random.value < chaoticTrafficChance;

        if (isChaoticTraffic)
        {
            Debug.Log("Tr�nsito ca�tico ATIVADO!");
            ApplyChaoticTrafficSettings();
        }
        else
        {
            Debug.Log("Tr�nsito ca�tico desativado nesta sess�o");
        }
    }

    private void ApplyChaoticTrafficSettings()
    {
        // SetChaoticMode foi removido do MultipleObjectSpawner
        // Modo caotico agora controlado apenas em TrafficSpawner e EnemySpawner
        Debug.Log("[GameplayManager] Modo caotico ativado para traffic");
    }

    private IEnumerator ExitGameplay(bool success = false)
    {
        yield return new WaitForSeconds(5);

        // Unload a cena Gameplay para liberar memoria
        Debug.Log("[GameplayManager] Descarregando Gameplay scene");
        SceneManager.UnloadSceneAsync("Gameplay");

        // Trocar para cena do menu
        if (SceneControl.instance != null) 
        {
            SceneControl.instance.ChangeScene("PoliceStation");
        }

        // Salvar progresso
        if (CareerPoints.instance != null) 
        {
            CareerPoints.instance.Save();
        }
    }

    /// <summary>
    /// Limpa todos os listeners de eventos
    /// </summary>
    private void CleanupListeners()
    {
        Debug.Log("[GameplayManager] Iniciando limpeza de listeners");

        // 1. Limpar listeners do PlayerCar
        if (playerCar != null && playerCar.carDamage != null)
        {
            playerCar.carDamage.onDamage = null;
            Debug.Log("[GameplayManager] onDamage listener removido");
        }

        // 2. Limpar listeners de todas as missions
        FastResponseMission fastResponse = FindObjectOfType<FastResponseMission>();
        if (fastResponse != null && playerCar != null && playerCar.carDamage != null)
        {
            playerCar.carDamage.onDamage = null;
        }

        // 3. Limpar listeners de PursuitMission
        PursuitMission pursuit = FindObjectOfType<PursuitMission>();
        if (pursuit != null)
        {
            // Limpar listeners de onDie de enemies
            EnemyCarFollowPath[] enemies = FindObjectsByType<EnemyCarFollowPath>(FindObjectsSortMode.None);
            foreach (EnemyCarFollowPath enemy in enemies)
            {
                if (enemy != null && enemy.damage != null)
                {
                    enemy.damage.onDie = null;
                }
            }
        }

        // 4. Limpar eventos de Scene Manager
        SceneManager.sceneLoaded -= OnSceneLoaded; // Remover se estava registrado

        Debug.Log("[GameplayManager] Limpeza de listeners completa");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Placeholder para remover listener
    }

    /// <summary>
    /// Audita problemas de duplica��o e ac�mulo ap�s GameOver
    /// </summary>
    public static void GameOverAudit()
    {
        Debug.Log("========== [AUDIT] Iniciando valida��o de ac�mulo ap�s GameOver ==========");

        // 1. Verificar duplica��o de Singletons
        Debug.Log("[AUDIT] Verificando Singletons...");
        PathGenerator.AuditInstances();
        if (CareerPoints.instance != null)
        {
            CareerPoints.AuditInstances();
        }

        // 2. Contar objetos de gameplay ainda ativos
        Debug.Log("[AUDIT] Contando objetos de gameplay...");
        TrafficCarFollowPath[] trafficCars = FindObjectsByType<TrafficCarFollowPath>(FindObjectsSortMode.None);
        Debug.Log($"[AUDIT] TrafficCars ativas: {trafficCars.Length}");

        TrafficSpawner[] trafficSpawners = FindObjectsByType<TrafficSpawner>(FindObjectsSortMode.None);
        Debug.Log($"[AUDIT] TrafficSpawners ativos: {trafficSpawners.Length}");

        EnemyCarFollowPath[] enemies = FindObjectsByType<EnemyCarFollowPath>(FindObjectsSortMode.None);
        Debug.Log($"[AUDIT] EnemyCars ativos: {enemies.Length}");

        // 3. Verificar coroutines ativas
        Debug.Log("[AUDIT] Verificando coroutines ativas...");
        foreach (TrafficSpawner spawner in trafficSpawners)
        {
            if (spawner != null)
            {
                Debug.LogWarning($"[AUDIT] TrafficSpawner ativo ainda est� rodando coroutines!");
            }
        }

        // 4. Verificar Rigidbodies com velocidades anormais
        Debug.Log("[AUDIT] Verificando Rigidbodies...");
        Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        int abnormalRigidbodies = 0;
        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb != null && !ValidationUtility.IsValidVelocity(rb.velocity))
            {
                abnormalRigidbodies++;
                Debug.LogWarning($"[AUDIT] Rigidbody com velocidade anormal: {rb.gameObject.name} - vel: {rb.velocity}");
            }
        }
        Debug.Log($"[AUDIT] Rigidbodies com velocidade anormal: {abnormalRigidbodies}/{rigidbodies.Length}");

        // 5. Verificar TimeScale
        Debug.Log($"[AUDIT] Time.timeScale: {Time.timeScale}");
        if (Time.timeScale != 1f)
        {
            Debug.LogWarning("[AUDIT] TimeScale nao esta em 1!");
        }

        // 6. Contar cenas ativas
        Debug.Log("[AUDIT] Cenas carregadas...");
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Debug.Log($"  [{i}] {scene.name} (objetos: {scene.rootCount})");
        }

        Debug.Log("========== [AUDIT] Valida��o conclu�da ==========");
    }

    /// <summary>
    /// Limpa completamente o PathGenerator
    /// </summary>
    private void CleanupPathGenerator()
    {
        PathGenerator pathGen = FindObjectOfType<PathGenerator>();
        if (pathGen != null)
        {
            // Parar todas as coroutines de gera��o
            pathGen.StopAllCoroutines();

            // Desativar valida��o de path
            pathGen.EnablePathValidation = false;

            Debug.Log("[GameplayManager] PathGenerator parado e valida��o desativada");
        }
    }

    /// <summary>
    /// Limpa completamente o TrafficSpawner
    /// </summary>
    private void CleanupTrafficSpawner()
    {
        TrafficSpawner trafficSpawner = FindObjectOfType<TrafficSpawner>();
        if (trafficSpawner != null)
        {
            // Parar todas as coroutines
            trafficSpawner.StopAllCoroutines();

            // Cancelar invokes
            trafficSpawner.CancelInvoke();

            Debug.Log("[GameplayManager] TrafficSpawner parado completamente");
        }
    }
}
