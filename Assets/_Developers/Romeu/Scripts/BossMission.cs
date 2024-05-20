using _Developers.Vitor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMission : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public EnemyCarFollowPath[] enemies;

    public EnemySpawner enemySpawner;

    List<EnemyCarFollowPath> enemyInstances = new();
    public int destroyedEnemies = 0;

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
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        enemyInstances.AddRange(enemySpawner.SpawnEnemies(enemies));

        foreach (EnemyCarFollowPath enemy in enemyInstances)
        {
            enemy.damage.onDie = () =>
            {
                destroyedEnemies++;
                enemy.gameObject.SetActive(false);
                if (destroyedEnemies >= enemies.Length)
                {
                    gameplayManager.EndGameplay(true);
                }
            };
        }
    }
}
