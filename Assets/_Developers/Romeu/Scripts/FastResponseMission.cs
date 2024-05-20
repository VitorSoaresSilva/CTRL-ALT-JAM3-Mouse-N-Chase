using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastResponseMission : MonoBehaviour
{
    public float lapsToWin = 4;
    public GameplayManager gameplayManager;
    public PlayerCar playerCar;
    public GameObject policeTunnelPrefab;

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
        
        playerCar.carDamage.onDamage = () =>
        {
            damageTaken++;
            if(damageTaken > 2)
            {
                gameplayManager.EndGameplay(false);
            }
        };
    }

    void Update()
    {
        if(gameplayManager.currentLap >= lapsToWin && !completed)
        {
            Debug.Log("Mission Complete");
            gameplayManager.EndGameplay(true);
            completed = true;
        }

        if(gameplayManager.currentLap >= lapsToWin - 1 && !tunnelSpawned)
        {
            PathGenerator.instance.tunnelPrefab = policeTunnelPrefab;
            Debug.Log("Changed tunnel prefab");
            tunnelSpawned = true;
        }
    }
}
