using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastResponseMission : MonoBehaviour
{
    public float lapsToWin = 4;
    public GameplayManager gameplayManager;
    public PlayerCar playerCar;
    
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
            gameplayManager.EndGameplay(false);
        };
    }

    void Update()
    {
        if(gameplayManager.currentLap >= lapsToWin)
        {
            Debug.Log("Mission Complete");
            gameplayManager.EndGameplay(true);
        }
    }
}
