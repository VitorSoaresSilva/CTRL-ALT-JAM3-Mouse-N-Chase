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
            // _pathGenerator.pathCreatorInstance.path.
            Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(40);
            EnemyCarFollowPath selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            EnemyCarFollowPath enemyInstance = Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
            enemyInstance.Init(_pathGenerator.carFollowPath, _pathGenerator.pathCreatorInstance, 40);
            enemyInstance.pathCreator = _pathGenerator.pathCreatorInstance;
        }

        public void SpawnEnemy()
        {

        }
    }
}