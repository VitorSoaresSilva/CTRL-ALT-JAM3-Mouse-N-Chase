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
        private PathGenerator _pathGenerator;

        public List<EnemyCarFollowPath> enemies = new();

        private void Awake()
        {
            _pathGenerator = GetComponent<PathGenerator>();
        }

        private void Start()
        {
            enemies.Clear();
        }

        public List<EnemyCarFollowPath> SpawnEnemies(EnemyCarFollowPath[] enemiesToSpawn)
        {
            enemies.Clear();

            if (enemiesToSpawn == null || enemiesToSpawn.Length == 0)
            {
                Debug.LogWarning("[EnemySpawner] Nenhum prefab de inimigo para spawnar.");
                return enemies;
            }

            if (_pathGenerator == null || _pathGenerator.pathCreatorInstance == null
                || _pathGenerator.pathCreatorInstance.path == null)
            {
                Debug.LogWarning("[EnemySpawner] Path não pronto para SpawnEnemies.");
                return enemies;
            }

            float playerDistance = 0f;
            if (_pathGenerator.carFollowPath != null)
                playerDistance = _pathGenerator.carFollowPath.distanceTravelled;

            // Espaçados à frente do player: +50, +120, +190...
            float[] aheadOffsets = { 50f, 120f, 190f, 260f };

            for (int i = 0; i < enemiesToSpawn.Length; i++)
            {
                float ahead = aheadOffsets[Mathf.Min(i, aheadOffsets.Length - 1)];
                // Se houver mais inimigos que offsets, incrementa
                if (i >= aheadOffsets.Length)
                    ahead = 50f + i * 70f;

                float spawnPos = playerDistance + ahead;
                Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(spawnPos);

                if (!ValidationUtility.IsValidVector3(spawnPosition))
                {
                    Debug.LogWarning($"[EnemySpawner] Posição de spawn inválida: {spawnPosition}. Pulando spawn.");
                    continue;
                }

                EnemyCarFollowPath enemyInstance = Instantiate(enemiesToSpawn[i], spawnPosition, Quaternion.identity);
                enemyInstance.Init(_pathGenerator.carFollowPath, _pathGenerator.pathCreatorInstance, spawnPos);
                enemyInstance.pathCreator = _pathGenerator.pathCreatorInstance;
                enemies.Add(enemyInstance);

                Debug.Log($"[EnemySpawner] Spawned {enemyInstance.name} at distance {spawnPos:F1}");
            }

            Debug.Log($"[EnemySpawner] Spawned {enemies.Count} enemies total");
            return enemies;
        }

        public List<EnemyCarFollowPath> SpawnRandomEnemies(EnemyCarFollowPath[] enemiesToSpawn, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(Random.Range(40, 100));

                if (!ValidationUtility.IsValidVector3(spawnPosition))
                {
                    Debug.LogWarning($"[EnemySpawner] Posição de spawn aleatória inválida: {spawnPosition}. Pulando spawn.");
                    continue;
                }

                EnemyCarFollowPath selectedEnemyPrefab = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Length)];
                EnemyCarFollowPath enemyInstance = Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);

                float spawnDist = Random.Range(40f, 100f);
                enemyInstance.Init(_pathGenerator.carFollowPath, _pathGenerator.pathCreatorInstance, spawnDist);
                enemyInstance.pathCreator = _pathGenerator.pathCreatorInstance;
                enemies.Add(enemyInstance);
            }

            return enemies;
        }
    }
}
