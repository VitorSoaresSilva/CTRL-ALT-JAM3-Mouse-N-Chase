using System;
using System.Collections;
using PathCreation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Developers.Vitor
{
    public class TrafficSpawner : MonoBehaviour
    {
        public TrafficCarFollowPath[] trafficCarPrefabs;
        // public float[] spawnPoints;
        public float minXOffset = -5f;
        public float maxXOffset = 5f;
        private PathGenerator _pathGenerator;
        public int amountSpawn = 20;
        public int minSpawnDist = 15;
        public int maxSpawnDist = 45;
        public bool goSameDirection = false;
        private int direction = 1;

        // Distâncias de segurança para evitar spawn perto do início e fim
        public float startSafetyDistance = 30f;
        public float endSafetyDistance = 30f;
        private GameplayManager _gameplayManager;

        private void Awake()
        {
            _pathGenerator = GetComponent<PathGenerator>();
            _gameplayManager = FindObjectOfType<GameplayManager>();
        }

        private void Start()
        {
            goSameDirection = Random.Range(0, 2) < 0.5f;
            direction = Random.Range(0, 2) < 0.5f ? -1 : 1;

            // Aguardar o fim da introdução antes de spawnar traffic
            StartCoroutine(SpawnTrafficAfterIntro());
        }

        private IEnumerator SpawnTrafficAfterIntro()
        {
            // Aguardar a introdução terminar
            if (_gameplayManager != null)
            {
                while (_gameplayManager.isIntroPlaying)
                {
                    yield return null;
                }
            }

            // Agora spawnar os carros de tráfego
            for (int i = 0; i < amountSpawn; i++)
            {
                SpawnTrafficCar(GetSafeSpawnDistance());
            }
        }

        private float GetSafeSpawnDistance()
        {
            // Gerar distância de spawn com zonas de segurança
            float pathLength = _pathGenerator.pathCreatorInstance.path.length;
            float safeMinDist = startSafetyDistance;
            float safeMaxDist = pathLength - endSafetyDistance;

            // Garantir que há espaço suficiente para spawn
            if (safeMaxDist <= safeMinDist)
            {
                safeMaxDist = pathLength;
            }

            // Gerar distância aleatória dentro da zona segura
            return Random.Range(safeMinDist, safeMaxDist);
        }

        private void SpawnTrafficCar(float spawnDistance)
        {
            Vector3 spawnPosition = _pathGenerator.pathCreatorInstance.path.GetPointAtDistance(spawnDistance, EndOfPathInstruction.Reverse);
            TrafficCarFollowPath selectedTrafficCarPrefab = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];
            TrafficCarFollowPath trafficCarInstance = Instantiate(selectedTrafficCarPrefab, spawnPosition, Quaternion.identity);
            trafficCarInstance.Init(_pathGenerator.pathCreatorInstance,
                Random.Range(0f, _pathGenerator.pathCreatorInstance.path.length),
                (goSameDirection) ? direction : Random.Range(0, 2) < 0.5f ? -1 : 1);

        }
    }
}
