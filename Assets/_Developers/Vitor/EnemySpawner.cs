using System;
using PathCreation;
using UnityEngine;

namespace _Developers.Vitor
{
    public class EnemySpawner : MonoBehaviour
    {
        public EnemyCarFollowPath enemyPrefab;
        // private PathCreator _pathCreator;
        private PathGenerator _pathGenerator;

        private void Awake()
        {
            // _pathCreator = GetComponent<PathCreator>();
            _pathGenerator = GetComponent<PathGenerator>();
        }

        private void Start()
        {
            // _pathGenerator.pathCreatorInstance.path.
            Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(40);
            EnemyCarFollowPath enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemyInstance.distanceTravelled = 40;
            enemyInstance.pathCreator = _pathGenerator.pathCreatorInstance;
        }

        private void Update()
        {
            
        }
    }
}