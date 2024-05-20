using System;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Developers.Vitor
{
    public class EnemySpawner : MonoBehaviour
    {
        public EnemyCarFollowPath[] enemyPrefabs;
        // private PathCreator _pathCreator;
        private PathGenerator _pathGenerator;

        public List<EnemyCarFollowPath> enemies = new();

        private void Awake()
        {
            // _pathCreator = GetComponent<PathCreator>();
            _pathGenerator = GetComponent<PathGenerator>();
        }

        private void Start()
        {
            enemies.Clear();
            // _pathGenerator.pathCreatorInstance.path
            //Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(40);
            //EnemyCarFollowPath selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            //EnemyCarFollowPath enemyInstance = Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
            //enemyInstance.Init(_pathGenerator.carFollowPath, _pathGenerator.pathCreatorInstance, 40);
            //enemyInstance.pathCreator = _pathGenerator.pathCreatorInstance;
        }

        public List<EnemyCarFollowPath> SpawnEnemies(EnemyCarFollowPath[] enemiesToSpawn)
        {
            float spawnPos = 40;

            foreach(EnemyCarFollowPath enemy in enemiesToSpawn)
            {
                Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(spawnPos);
                EnemyCarFollowPath enemyInstance = Instantiate(enemy, spawnPosition, Quaternion.identity);
                enemyInstance.Init(_pathGenerator.carFollowPath, _pathGenerator.pathCreatorInstance, 40);
                enemyInstance.pathCreator = _pathGenerator.pathCreatorInstance;
                enemies.Add(enemyInstance);

                spawnPos += 5;
            }

            return enemies;
        }

        public List<EnemyCarFollowPath> SpawnRandomEnemies(EnemyCarFollowPath[] enemiesToSpawn, int quantity = 1)
        {
            //float spawnPos = 40;
            for (int i = 0; i < quantity; i++)
            {
                Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(Random.Range(40, 100));

                EnemyCarFollowPath selectedEnemyPrefab = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Length)];
                EnemyCarFollowPath enemyInstance = Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
                
                enemyInstance.Init(_pathGenerator.carFollowPath, _pathGenerator.pathCreatorInstance, 40);
                enemyInstance.pathCreator = _pathGenerator.pathCreatorInstance;
                enemies.Add(enemyInstance);
                
                //spawnPos += 5;
            }

            return enemies;
        }
    }
}