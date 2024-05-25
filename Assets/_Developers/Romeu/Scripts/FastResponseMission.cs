using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastResponseMission : MonoBehaviour
{
    public float lapsToWin = 4;
    public GameplayManager gameplayManager;
    public PlayerCar playerCar;
    public GameObject policeTunnelPrefab;
    public AudioClip damageAudio;
    private AudioSource audioSource;
    [SerializeField] private PoliceTunnel policeTunnel;

    public int damageTaken = 0;
    public bool tunnelSpawned = false;
    public bool completed = false;
    
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

        policeTunnel = FindObjectOfType<PoliceTunnel>();

        playerCar.carDamage.onDamage = () =>
        {
            damageTaken++;
            //Debug.Log("Damage taken");
            if(damageTaken == 4)
            {
                audioSource.PlayOneShot(damageAudio);
            }
            if(damageTaken > 5)
            {
                gameplayManager.EndGameplay(false);
            }
        };
    }

    void Update()
    {
        if(gameplayManager.currentLap >= lapsToWin && !completed)
        {
            //Debug.Log("Mission Complete");
            gameplayManager.EndGameplay(true);
            completed = true;
        }

        if (gameplayManager.currentLap >= lapsToWin - 1 && !tunnelSpawned)
        {
            policeTunnel.SetActive(true);
            //PathGenerator.instance.tunnelPrefab = policeTunnelPrefab;
            //Debug.Log("Changed tunnel prefab");
            Debug.Log("Tunnel spawned");
            tunnelSpawned = true;
        }
    }
}
